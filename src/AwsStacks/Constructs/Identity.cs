using Amazon.CDK;
using AwsStacks.Models;
using AwsStacks.StackElements;

namespace AwsStacks.Constructs
{
    public class Identity : Construct
    {
        public Identity(Construct scope, string id, ProjectStackProps stackProps) : base(scope, id)
        {
            var cognito = new CognitoStack(this, "cognito", stackProps);
        }
    }
}