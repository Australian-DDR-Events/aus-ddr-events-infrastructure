using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.Route53.Targets;
using Amazon.CDK.AWS.SSM;
using Constructs;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace CognitoEnhanced.CognitoConstructs {
    public class MigrationCognito : Construct {
        public UserPool UserPool { get; private set; }
        
        public UserPoolDomainTarget UserPoolDomainTarget {get; private set;}
        
        public MigrationCognito(Construct scope, string id, MigrationCognitoProps props) : base(scope, id) {
            var userMigrationLambda = CreateUserMigrationLambda(props.UserMigrationLambda);

            UserPool = CreateUserPool(userMigrationLambda);

            foreach (var (userPoolGroupDefinition, precedence) in props.UserPoolGroups.Select((groupDefinition, i) => (groupDefinition, i)))
            {
                CreateGroup(userPoolGroupDefinition, precedence, UserPool.UserPoolId);
            }

            IDictionary<string, Tuple<UserPoolResourceServer, UserPoolResourceServerOptions>> resourceServers = new Dictionary<string, Tuple<UserPoolResourceServer, UserPoolResourceServerOptions>>();
            var resourceServerOptionEnumerable = props
                .ResourceServers
                .Select(CreateResourceServer)
                .Select(rs => new Tuple<string, UserPoolResourceServerOptions>(rs.Identifier, rs));
            foreach (var (identifier, resourceServerOptions) in resourceServerOptionEnumerable)
            {
                var resourceServer =UserPool.AddResourceServer(identifier, resourceServerOptions);
                resourceServers.Add(identifier, new Tuple<UserPoolResourceServer, UserPoolResourceServerOptions>(resourceServer, resourceServerOptions));
            }

            var userClients = props
                .UserClients
                .Select(c => CreateClient(c, resourceServers))
                .Select(cl => new Tuple<string, UserPoolClientOptions>(cl.UserPoolClientName, cl));
            foreach (var (identifier, clientOptions) in userClients)
            {
                UserPool.AddClient(identifier, clientOptions);
            }
            var serviceClients = props
                .ServiceClients
                .Select(c => CreateServiceClient(c, resourceServers))
                .Select(cl => new Tuple<string, UserPoolClientOptions>(cl.UserPoolClientName, cl));
            foreach (var (identifier, clientOptions) in serviceClients)
            {
                UserPool.AddClient(identifier, clientOptions);
            }

            var domain = new UserPoolDomainOptions() {
                CustomDomain = new CustomDomainOptions() {
                    DomainName = props.Domain,
                    Certificate = Certificate.FromCertificateArn(this, "cognito-domain-certificate", props.SslArn),
                }
            };
            UserPool.AddDomain("user-pool-domain-association", domain);
            
            PerformUnsafeOperations(props);


            // var userPoolDomain = new UserPoolDomain(this, "user-pool-domain", new UserPoolDomainProps {
            //     UserPool = userPool,
            //     CustomDomain = new CustomDomainOptions() {
            //         DomainName = props.Domain,
            //         Certificate = Certificate.FromCertificateArn(this, "cognito-domain-certificate", props.SslArn),
            //     },
            // });
        }

        private Table CreateMigrationTable() {
            return new Table(this, "cognito-migration-table", new TableProps() {
                BillingMode = BillingMode.PAY_PER_REQUEST,
                PartitionKey = new Attribute {
                    Name = "email",
                    Type = AttributeType.STRING,
                },
            });
        }

        private Function CreateUserMigrationLambda(UserMigrationLambdaProps props) {
            var firebaseApiKey = StringParameter.ValueForStringParameter(this, props.FirebaseApiKeyPath);

            var lambda = new Function(this, "user-pool-trigger", new FunctionProps() {
                Code = Code.FromAsset(Path.Combine(props.ResourcesPath, "UserMigrationTrigger/out.zip")),
                Runtime = Runtime.DOTNET_6,
                Handler = "MigrateUserTrigger::MigrateUserTrigger.Function::FunctionHandler",
                Timeout = Duration.Seconds(5),
                LogRetention = RetentionDays.ONE_DAY,
                Environment = new Dictionary<string, string>
                {
                    {"FIREBASE_API_KEY", firebaseApiKey},
                },
                MemorySize = 512
            });
            
            var migrationTable = CreateMigrationTable();
            migrationTable.GrantReadData(lambda);
            lambda.AddEnvironment("USER_TABLE_NAME", migrationTable.TableName);
            lambda.AddEnvironment("FIREBASE_API_KEY", firebaseApiKey);

            return lambda;
        }

        private UserPool CreateUserPool(Function userMigrationFunction) {
            return new UserPool(this, "userPool", new UserPoolProps
            {
                AccountRecovery = AccountRecovery.EMAIL_AND_PHONE_WITHOUT_MFA,
                SignInAliases = new SignInAliases
                {
                    Email = true
                },
                CustomAttributes = new Dictionary<string, ICustomAttribute>
                {
                    {"legacy_id", new StringAttribute()}
                },
                Mfa = Mfa.OPTIONAL,
                MfaSecondFactor = new MfaSecondFactor
                {
                    Otp = true,
                    Sms = false
                },
                LambdaTriggers = new UserPoolTriggers
                {
                    UserMigration = userMigrationFunction,
                    
                },
                SelfSignUpEnabled = true,
                DeviceTracking = new DeviceTracking()
                {
                    ChallengeRequiredOnNewDevice = true,
                    DeviceOnlyRememberedOnUserPrompt = false
                },
                StandardAttributes = new StandardAttributes
                {
                    Email = new StandardAttribute
                    {
                        Mutable = true,
                        Required = true,
                    },
                    Nickname = new StandardAttribute
                    {
                        Mutable = true,
                        Required = true,
                    }
                },
                SignInCaseSensitive = false
            });
        }

        private UserPoolResourceServerOptions CreateResourceServer(ResourceServerDefinition resourceServerProps) {
            return new UserPoolResourceServerOptions() {
                Identifier = resourceServerProps.Identifier,
                Scopes = resourceServerProps.Scopes.Select(s => new ResourceServerScope(new ResourceServerScopeProps() {
                    ScopeDescription = s.Description,
                    ScopeName = s.Name,
                })).ToArray()
            };
        }

        private UserPoolClientOptions CreateClient(UserClientDefinition client, IDictionary<string, Tuple<UserPoolResourceServer, UserPoolResourceServerOptions>> resourceServers)
        {
            // var scopes = client.Scopes.Select(s =>
            //     {
            //         var (userPoolResourceServer, userPoolResourceServerOptions) =
            //             resourceServers[s.ResourceServerIdentifier];
            //
            //         return s.Scopes.Select(scopeName =>
            //             OAuthScope.ResourceServer(userPoolResourceServer,
            //                 userPoolResourceServerOptions.Scopes.First(rss => rss.ScopeName.Equals(scopeName))));
            //     }).SelectMany(x => x)
            //     .ToList();
            //
            var scopes = new List<OAuthScope>();
            scopes.AddRange(new List<OAuthScope>(){OAuthScope.OPENID, OAuthScope.EMAIL, OAuthScope.PROFILE});

            return new UserPoolClientOptions()
            {
                UserPoolClientName = client.Identifier,
                OAuth = new OAuthSettings
                {
                    CallbackUrls = client.CallbackUrls.ToArray(),
                    Flows = new OAuthFlows
                    {
                        ClientCredentials = false,
                        AuthorizationCodeGrant = true,
                        ImplicitCodeGrant = false
                    },
                    LogoutUrls = client.LogoutUrls.ToArray(),
                    Scopes = scopes.ToArray()
                },
                GenerateSecret = client.UseBackend
            };
        }

        private UserPoolClientOptions CreateServiceClient(ServiceClientDefinition client,
            IDictionary<string, Tuple<UserPoolResourceServer, UserPoolResourceServerOptions>> resourceServers)
        {
            var scopes = client
                .Scopes
                .Select(s => 
                    s.Scopes.Where(ss =>
                            (resourceServers.FirstOrDefault(r => r.Key.Equals(s.ResourceServerIdentifier)).Value.Item2.Scopes ?? Array.Empty<ResourceServerScope>())
                            .Any(rss => rss.ScopeName.Equals(ss))
                        ).Select(ss => $"{s.ResourceServerIdentifier}/{ss}")
                )
                .SelectMany(s => s);
            return new UserPoolClientOptions()
            {
                UserPoolClientName = client.Identifier,
                OAuth = new OAuthSettings()
                {
                    Flows = new OAuthFlows()
                    {
                        ClientCredentials = true,
                        AuthorizationCodeGrant = false,
                        ImplicitCodeGrant = false
                    },
                    Scopes = scopes.Select(OAuthScope.Custom).ToArray()
                },
                GenerateSecret = true
            };
        }

        private CfnUserPoolGroup CreateGroup(UserPoolGroupDefinition groupDefinition, int precedence, string userPoolId)
        {
            return new CfnUserPoolGroup(this, $"userpool-group-{groupDefinition.GroupName}", new CfnUserPoolGroupProps
            {
                Description = groupDefinition.Description,
                GroupName = groupDefinition.GroupName,
                Precedence = precedence,
                UserPoolId = userPoolId
            });
        }

        private void PerformUnsafeOperations(MigrationCognitoProps props)
        {
            var cfnUserPool = (CfnUserPool) UserPool.Node.DefaultChild;
            cfnUserPool.EmailConfiguration = new CfnUserPool.EmailConfigurationProperty
            {
                SourceArn = props.SesArn,
                EmailSendingAccount = "DEVELOPER",
                From = props.EmailFromAddress
            };
            
        }
    }
}