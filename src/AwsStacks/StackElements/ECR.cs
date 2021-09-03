using Amazon.CDK;
using Amazon.CDK.AWS.ECR;

namespace AwsStacks.StackElements
{
    public class ECRStack : Stack
    {
        public ECRStack(Construct scope, string id, IStackProps props) : base(scope, id, props)
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
        }
    }
}