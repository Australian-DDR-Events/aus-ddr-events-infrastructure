using Amazon.CDK;
using AwsStacks.Models;
using AwsStacks.Stacks;

namespace AwsStacks.Constructs
{
    public class Core : Construct
    {
        public Core(Construct scope, string id, ProjectStackProps stackProps) : base(scope, id)
        {
            new ECRStack(this, "aus-ddr-events-api-ecr-stack", stackProps);

            new CdnStack(this, $"aus-ddr-events-cdn-{stackProps.ProjectEnvironment.env}-stack", stackProps);
        }
    }
}