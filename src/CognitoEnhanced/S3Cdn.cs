using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SSM;
using CognitoEnhanced.S3CdnConstructs;
using Constructs;
using CloudFrontProxy = CognitoEnhanced.S3WebAppConstructs.CloudFrontProxy;

namespace CognitoEnhanced
{
    public class S3Cdn : Stack
    {
        public S3Cdn(Construct scope, string id, AppStackProps props, Role ec2Role) : base(scope, id, props)
        {
            var cdnBucket = new CdnBucket(this, "cdn-bucket", props.cdnBucket);
            var proxy = new CdnCloudFrontProxy(this, "cdn-proxy", cdnBucket.Cdn, props.cdnProxy);

            cdnBucket.Cdn.GrantReadWrite(ec2Role);
            cdnBucket.Cdn.GrantDelete(ec2Role);
            cdnBucket.Cdn.GrantPut(ec2Role);

            new StringParameter(this, "stack-name", new StringParameterProps
            {
                ParameterName = $"/{props.label}/cdn/id",
                StringValue = id
            });

            ExportValue(cdnBucket.Cdn.BucketName, new ExportValueOptions
            {
                Name = $"{props.label}-cdn-bucket-name"
            });

            ExportValue(proxy.CdnDistribution.DistributionId, new ExportValueOptions
            {
                Name = $"{props.label}-cdn-proxy-id"
            });
        }
    }
}