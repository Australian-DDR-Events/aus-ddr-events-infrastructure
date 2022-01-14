using Amazon.CDK.AWS.S3;

namespace CognitoEnhanced.S3WebAppConstructs
{
    public class AppBucketProps : BucketProps
    {
        public string App { get; set; }
    }
}