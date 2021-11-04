using System.Collections.Generic;
using System.IO;
using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SSM;
using AwsStacks.Models;
using AwsStacks.StackItems;

namespace AwsStacks.Stacks
{
    public class CognitoStack : Stack
    {
        public IUserPool UserPool { get; }

        public CognitoStack(Construct scope, string id, ProjectStackProps props) : base(scope, id, props)
        {
            var apiKey = StringParameter.ValueForStringParameter(
                this, 
                $"{props.ProjectEnvironment.cognito.ssmPrefix}firebase-api-key");
            var credential = StringParameter.ValueForStringParameter(
                this, 
                $"{props.ProjectEnvironment.cognito.ssmPrefix}firebase-credential");
            
            UserPool = new UserPool(this, "userPool", new UserPoolProps
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
                    UserMigration = new Function(this, "userPoolTrigger", new FunctionProps()
                    {
                        Code = Code.FromAsset(Path.Combine(props.ProjectEnvironment.resourcesPath, "cognito/UserMigrationTrigger/out.zip")),
                        Runtime = Runtime.DOTNET_CORE_3_1,
                        Handler = "MigrateUserTrigger::MigrateUserTrigger.Function::FunctionHandler",
                        Timeout = Duration.Seconds(5),
                        LogRetention = RetentionDays.ONE_DAY,
                        Environment = new Dictionary<string, string>
                        {
                            {"FIREBASE_API_KEY", apiKey},
                            {"FIREBASE_CREDENTIAL", credential}
                        },
                        MemorySize = 512
                    }),
                    
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
                        Mutable = false,
                        Required = true
                    },
                    Nickname = new StandardAttribute
                    {
                        Mutable = true,
                        Required = true
                    }
                },
                SignInCaseSensitive = false
            });

            var ausDdrEventsApi = UserPool.AddResourceServer("aus-ddr-events-api", new UserPoolResourceServerOptions
            {
                Identifier = "aus-ddr-events-api",
                Scopes = new[]
                {
                    new ResourceServerScope(new ResourceServerScopeProps
                    {
                        ScopeDescription = "Read",
                        ScopeName = "user:read"
                    })
                }
            });
            
            UserPool.AddClient("pkce-test-app", new UserPoolClientOptions
            {
                OAuth = new OAuthSettings
                {
                    CallbackUrls = new []
                    {
                        "http://localhost:3000",
                        "http://localhost:1234/callback"
                    },
                    Flows = new OAuthFlows
                    {
                        ClientCredentials = false,
                        AuthorizationCodeGrant = true,
                        ImplicitCodeGrant = false
                    },
                    LogoutUrls = new []
                    {
                        "http://localhost:3000/logout",
                        "http://localhost:1234/logout"
                    },
                    Scopes = new []
                    {
                        OAuthScope.ResourceServer(ausDdrEventsApi, new ResourceServerScope(new ResourceServerScopeProps
                        {
                            ScopeDescription = "Read",
                            ScopeName = "user:read"
                        })), 
                        OAuthScope.EMAIL,
                        OAuthScope.OPENID, 
                        OAuthScope.PROFILE
                    }
                },
                GenerateSecret = false
            });

            UserPool.AddClient("aus-ddr-events-staging", new UserPoolClientOptions
            {
                OAuth = new OAuthSettings
                {
                    CallbackUrls = new []
                    {
                        "https://stg.ausddrevents.com",
                        "https://stg.ausddrevents.com/login"
                    },
                    Flows = new OAuthFlows
                    {
                        ClientCredentials = false,
                        AuthorizationCodeGrant = true,
                        ImplicitCodeGrant = false
                    },
                    LogoutUrls = new []
                    {
                        "https://stg.ausddrevents.com/logout"
                    },
                },
                GenerateSecret = false
            });

            UserPool.AddClient("aus-ddr-events-production", new UserPoolClientOptions
            {
                OAuth = new OAuthSettings
                {
                    CallbackUrls = new []
                    {
                        "https://ausddrevents.com",
                        "https://ausddrevents.com/login",
                        "https://www.ausddrevents.com",
                        "https://www.ausddrevents.com/login"
                    },
                    Flows = new OAuthFlows
                    {
                        ClientCredentials = false,
                        AuthorizationCodeGrant = true,
                        ImplicitCodeGrant = false
                    },
                    LogoutUrls = new []
                    {
                        "https://ausddrevents.com/logout",
                        "https://www.ausddrevents.com/logout"
                    },
                },
                GenerateSecret = false
            });
            
            var discordClient = UserPool.AddClient("aus-ddr-discord", new UserPoolClientOptions
            {
                OAuth = new OAuthSettings
                {
                    Flows = new OAuthFlows
                    {
                        ClientCredentials = true,
                        AuthorizationCodeGrant = false,
                        ImplicitCodeGrant = false
                    },
                    Scopes = new []
                    {
                        OAuthScope.ResourceServer(ausDdrEventsApi, new ResourceServerScope(new ResourceServerScopeProps
                        {
                            ScopeDescription = "Read",
                            ScopeName = "user:read"
                        })), 
                    }
                },
                GenerateSecret = true
            });

            UserPool.AddDomain("ausddrevents", new UserPoolDomainOptions
            {
                // CustomDomain = new CustomDomainOptions
                // {
                //     DomainName = props.ProjectEnvironment.cognito.domain,
                //     Certificate = Certificate.FromCertificateArn(this, "certificate", props.ProjectEnvironment.cognito.sslCertificateArn)
                // },
                CognitoDomain = new CognitoDomainOptions
                {
                    DomainPrefix = "bauxe-test"
                }
            });

            new CfnUserPoolGroup(this, "admin-group", new CfnUserPoolGroupProps
            {
                Description = "AusDdrEvents Administrators",
                GroupName = "Administrators",
                Precedence = 1,
                UserPoolId = UserPool.UserPoolId
            });

            var cfnUserPool = (CfnUserPool) UserPool.Node.DefaultChild;
            cfnUserPool.EmailConfiguration = new CfnUserPool.EmailConfigurationProperty
            {
                SourceArn = props.ProjectEnvironment.cognito.sesArn,
                EmailSendingAccount = "DEVELOPER",
                From = props.ProjectEnvironment.cognito.emailFromAddress
            };

            StackParameter.AddStackParameter(this, "cognito", props);
        }

    }
}