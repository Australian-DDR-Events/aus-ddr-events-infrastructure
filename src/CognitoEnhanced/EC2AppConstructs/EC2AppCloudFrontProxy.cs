using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace CognitoEnhanced.EC2AppConstructs {
    public class EC2AppCloudFrontProxy : Construct {
        public Distribution EC2AppDistribution { get; private set; }

        public EC2AppCloudFrontProxy(Construct scope, string id, IInstance ec2Instance, EC2AppCloudFrontProxyProps props = null) : base(scope, id)
        {
            EC2AppDistribution = CreateDistribution(ec2Instance, props);
        }

        private Distribution CreateDistribution(IInstance ec2Instance, EC2AppCloudFrontProxyProps props)
        {
            var ec2AppOrigin = new HttpOrigin(ec2Instance.InstancePublicDnsName, new HttpOriginProps
            {
                HttpPort = 5000,
                ProtocolPolicy = OriginProtocolPolicy.HTTP_ONLY
            });
            return new Distribution(this, "ec2app-proxy", new DistributionProps() {
                DefaultBehavior = new BehaviorOptions
                {
                    AllowedMethods = AllowedMethods.ALLOW_ALL,
                    Origin = ec2AppOrigin,
                    CachedMethods = CachedMethods.CACHE_GET_HEAD_OPTIONS,
                    Compress = true,
                    CachePolicy = CachePolicy.CACHING_DISABLED,
                    OriginRequestPolicy = OriginRequestPolicy.ALL_VIEWER
                },
                Certificate = Certificate.FromCertificateArn(this, "ec2app-domain-certificate", props.SslArn),
                DomainNames = new []{props.Domain},
                EnableLogging = false,
                PriceClass = PriceClass.PRICE_CLASS_ALL,
                DefaultRootObject = "/",
            });
        }
    }
}