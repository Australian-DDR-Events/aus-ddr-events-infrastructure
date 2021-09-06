using Amazon.CDK;
using Amazon.CDK.AWS.SSM;
using AwsStacks.Models;

namespace AwsStacks.StackItems
{
    public static class StackParameter
    {
        public static StringParameter AddStackParameter(Stack scope, string stackIdentifier, ProjectStackProps props)
        {
            var env = props.ProjectEnvironment.env;
            return new StringParameter(scope, $"cdn-stack-id-{env}", new StringParameterProps
            {
                ParameterName = $"/{env}/stacks/{stackIdentifier}",
                StringValue = scope.StackId,
                Tier = ParameterTier.STANDARD,
                Type = ParameterType.STRING
            });
        }
    }
}