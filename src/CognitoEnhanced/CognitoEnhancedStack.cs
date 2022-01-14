using Amazon.CDK;
using Amazon.CDK.AWS.SSM;
using CognitoEnhanced.CognitoConstructs;
using Constructs;

namespace CognitoEnhanced
{
    public class CognitoEnhancedStack : Stack
    {
        internal CognitoEnhancedStack(Construct scope, string id, AppStackProps props) : base(scope, id, props)
        {
            var migrationCognito = new MigrationCognito(this, "migration-cognito", props.migrationCognito);

            new StringParameter(this, "stack-name", new StringParameterProps
            {
                ParameterName = $"/{props.label}/cognito/id",
                StringValue = id
            });
        }
    }
}
