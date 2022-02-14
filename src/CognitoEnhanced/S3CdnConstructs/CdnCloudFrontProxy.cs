using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace CognitoEnhanced.S3CdnConstructs {
    public class CdnCloudFrontProxy : Construct {
        public Distribution CdnDistribution { get; private set; }

        public CdnCloudFrontProxy(Construct scope, string id, IBucket cdnBucket, CdnCloudFrontProxyProps props = null) : base(scope, id)
        {
            CdnDistribution = CreateDistribution(cdnBucket, props);
        }

        private Distribution CreateDistribution(IBucket cdnBucket, CdnCloudFrontProxyProps props) {
            var cdnOrigin = new S3Origin(cdnBucket);
            return new Distribution(this, "cdn-proxy", new DistributionProps() {
                DefaultBehavior = new BehaviorOptions
                {
                    AllowedMethods = AllowedMethods.ALLOW_GET_HEAD_OPTIONS,
                    Origin = cdnOrigin,
                    CachedMethods = CachedMethods.CACHE_GET_HEAD_OPTIONS,
                    Compress = true,
                    CachePolicy = new CachePolicy(this, "cdn-cache-policy", new CachePolicyProps
                    {
                        EnableAcceptEncodingBrotli = true,
                        EnableAcceptEncodingGzip = true
                    }),
                    OriginRequestPolicy = OriginRequestPolicy.CORS_S3_ORIGIN
                },
                Certificate = Certificate.FromCertificateArn(this, "cdn-domain-certificate", props.SslArn),
                DomainNames = new []{props.Domain},
                EnableLogging = false,
                PriceClass = PriceClass.PRICE_CLASS_ALL
            });
        }
    }
}