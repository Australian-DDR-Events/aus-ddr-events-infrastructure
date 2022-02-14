using Amazon.CDK.AWS.S3;

namespace CognitoEnhanced.S3CdnConstructs
{
    public class CdnBucketProps : BucketProps
    {
        public string Cdn { get; set; }
    }
}