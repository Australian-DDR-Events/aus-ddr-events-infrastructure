using Amazon.CDK.AWS.CloudFront;

namespace CognitoEnhanced.EC2AppConstructs {
    public class EC2AppCloudFrontProxyProps : DistributionProps {
        public string Domain { get; set; }
        public string SslArn { get; set; }
    }
}