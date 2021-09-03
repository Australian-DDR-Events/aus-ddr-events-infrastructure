using System.Collections.Generic;
using System.IO;
using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.SSM;
using AwsStacks.Models;

namespace AwsStacks.StackElements
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
                    {"legacy_id", new StringAttribute()},
                    {"internal_id", new StringAttribute()}
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
                        Environment = new Dictionary<string, string>
                        {
                            {"FIREBASE_API_KEY", apiKey},
                            {"FIREBASE_CREDENTIAL", credential}
                        },
                        MemorySize = 128
                    }),
                    
                },
                SelfSignUpEnabled = true,
                DeviceTracking = new DeviceTracking()
                {
                    ChallengeRequiredOnNewDevice = true,
                    DeviceOnlyRememberedOnUserPrompt = false
                }
            });

            UserPool.AddClient("pkce-test-app", new UserPoolClientOptions
            {
                OAuth = new OAuthSettings
                {
                    CallbackUrls = new []
                    {
                        "http://localhost:3000"
                    },
                    Flows = new OAuthFlows
                    {
                        ClientCredentials = false,
                        AuthorizationCodeGrant = true,
                        ImplicitCodeGrant = false
                    },
                    LogoutUrls = new []
                    {
                        "http://localhost:3000/logout"
                    },
                },
                GenerateSecret = false
            });

            UserPool.AddResourceServer("aus-ddr-events-api", new UserPoolResourceServerOptions
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

            UserPool.AddDomain("ausddrevents", new UserPoolDomainOptions
            {
                CustomDomain = new CustomDomainOptions
                {
                    DomainName = props.ProjectEnvironment.cognito.domain,
                    Certificate = Certificate.FromCertificateArn(this, "certificate", props.ProjectEnvironment.cognito.sslCertificateArn)
                }
            });

            var cfnUserPool = (CfnUserPool) UserPool.Node.DefaultChild;
            cfnUserPool.EmailConfiguration = new CfnUserPool.EmailConfigurationProperty
            {
                SourceArn = props.ProjectEnvironment.cognito.sesArn,
                EmailSendingAccount = "DEVELOPER",
                From = props.ProjectEnvironment.cognito.emailFromAddress
            };
        }

    }
}