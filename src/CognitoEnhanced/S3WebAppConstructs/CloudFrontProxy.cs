using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace CognitoEnhanced.S3WebAppConstructs {
    public class CloudFrontProxy : Construct {
        public Distribution WebappDistribution { get; private set; }

        public CloudFrontProxy(Construct scope, string id, IBucket webappBucket, CloudFrontProxyProps props = null) : base(scope, id)
        {
            WebappDistribution = CreateDistribution(webappBucket, props);
        }

        private Distribution CreateDistribution(IBucket webappBucket, CloudFrontProxyProps props) {
            var webappOrigin = new S3Origin(webappBucket);
            return new Distribution(this, "webapp-proxy", new DistributionProps() {
                DefaultBehavior = new BehaviorOptions
                {
                    AllowedMethods = AllowedMethods.ALLOW_GET_HEAD_OPTIONS,
                    Origin = webappOrigin,
                    CachedMethods = CachedMethods.CACHE_GET_HEAD_OPTIONS,
                    Compress = true,
                    CachePolicy = new CachePolicy(this, "webapp-cache-policy", new CachePolicyProps
                    {
                        EnableAcceptEncodingBrotli = true,
                        EnableAcceptEncodingGzip = true
                    })
                },
                Certificate = Certificate.FromCertificateArn(this, "webapp-domain-certificate", props.SslArn),
                DomainNames = new []{props.Domain},
                EnableLogging = false,
                PriceClass = PriceClass.PRICE_CLASS_ALL,
                DefaultRootObject = "index.html",
                ErrorResponses = new []
                {
                    new ErrorResponse
                    {
                        HttpStatus = 404,
                        ResponseHttpStatus = 200,
                        ResponsePagePath = "/index.html"
                    },
                    new ErrorResponse
                    {
                        HttpStatus = 403,
                        ResponseHttpStatus = 200,
                        ResponsePagePath = "/index.html"
                    }
                }
            });
        }
    }
}