using Amazon.CDK;
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
        }
    }
}