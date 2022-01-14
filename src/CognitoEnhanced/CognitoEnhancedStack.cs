using Amazon.CDK;
using CognitoEnhanced.CognitoConstructs;
using Constructs;

namespace CognitoEnhanced
{
    public class CognitoEnhancedStack : Stack
    {
        internal CognitoEnhancedStack(Construct scope, string id, AppStackProps props) : base(scope, id, props)
        {
            var migrationCognito = new MigrationCognito(this, "migration-cognito", props.migrationCognito);
        }
    }
}
