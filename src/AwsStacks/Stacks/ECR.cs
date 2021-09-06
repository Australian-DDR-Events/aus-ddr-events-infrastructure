using Amazon.CDK;
using Amazon.CDK.AWS.ECR;
using AwsStacks.Models;
using AwsStacks.StackItems;

namespace AwsStacks.Stacks
{
    public class ECRStack : Stack
    {
        public ECRStack(Construct scope, string id, ProjectStackProps props) : base(scope, id, props)
        {
            var ecr = new Repository(this, "ecr", new RepositoryProps
            {
                ImageScanOnPush = false,
                LifecycleRules = new[]
                {
                    new LifecycleRule
                    {
                        MaxImageAge = Duration.Days(7)
                    }
                }
            });

            ExportValue(ecr.RepositoryUri, new ExportValueOptions
            {
                Name = "ecr-uri"
            });
            
            StackParameter.AddStackParameter(this, "ecr", props);
        }
    }
}