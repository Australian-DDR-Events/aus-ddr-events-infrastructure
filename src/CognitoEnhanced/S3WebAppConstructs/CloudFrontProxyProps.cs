using Amazon.CDK.AWS.CloudFront;

namespace CognitoEnhanced.S3WebAppConstructs {
    public class CloudFrontProxyProps : DistributionProps {
        public string Domain { get; set; }
        public string SslArn { get; set; }
    }
}