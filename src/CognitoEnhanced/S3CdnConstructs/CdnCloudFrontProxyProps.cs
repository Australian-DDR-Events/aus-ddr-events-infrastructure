using Amazon.CDK.AWS.CloudFront;

namespace CognitoEnhanced.S3CdnConstructs {
    public class CdnCloudFrontProxyProps : DistributionProps {
        public string Domain { get; set; }
        public string SslArn { get; set; }
    }
}