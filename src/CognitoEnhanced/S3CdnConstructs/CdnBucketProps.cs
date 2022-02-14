using System.Collections.Generic;
using Amazon.CDK.AWS.S3;

namespace CognitoEnhanced.S3CdnConstructs
{
    public class CdnBucketProps : BucketProps
    {
        public IList<string> CorsOrigins { get; set; }
    }
}