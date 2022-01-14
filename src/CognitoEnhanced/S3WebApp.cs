using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.SSM;
using CognitoEnhanced.S3WebAppConstructs;
using Constructs;

namespace CognitoEnhanced
{
    public class S3WebApp : Stack
    {
        public S3WebApp(Construct scope, string id, AppStackProps props) : base(scope, id, props)
        {
            var appBucket = new AppBucket(this, "webapp-bucket", props.appBucket);
            var proxy = new CloudFrontProxy(this, "webapp-proxy", appBucket.WebappBucket, props.cloudFrontProxy);

            new StringParameter(this, "stack-name", new StringParameterProps
            {
                ParameterName = $"/{props.label}/webapp/id",
                StringValue = id
            });

            ExportValue(appBucket.WebappBucket.BucketName, new ExportValueOptions
            {
                Name = $"{props.label}-bucket-name"
            });

            ExportValue(proxy.WebappDistribution.DistributionId, new ExportValueOptions
            {
                Name = $"{props.label}-proxy-id"
            });
        }
    }
}