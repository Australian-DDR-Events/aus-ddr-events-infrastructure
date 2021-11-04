using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.SSM;
using AwsStacks.Models;
using AwsStacks.StackItems;

namespace AwsStacks.Stacks
{
    public class CdnStack : Stack
    {
        public CdnStack(Construct scope, string id, ProjectStackProps props) : base(scope, id, props)
        {
            var env = props.ProjectEnvironment.env;
            var assetsBucket = new Bucket(this, $"cdn-assets-bucket-{env}", new BucketProps
            {
                AccessControl = BucketAccessControl.PRIVATE,
                AutoDeleteObjects = false,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                PublicReadAccess = false
            });
            
            var rootBucket = new Bucket(this, $"cdn-root-bucket-{env}", new BucketProps
            {
                AccessControl = BucketAccessControl.PRIVATE,
                AutoDeleteObjects = false,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                PublicReadAccess = false
            });

            var policy = new Policy(this, $"cdn-access-policy-{env}", new PolicyProps
            {
                Statements = new[]
                {
                    new PolicyStatement(new PolicyStatementProps
                    {
                        Resources = new[]
                        {
                            assetsBucket.BucketArn,
                            rootBucket.BucketArn
                        },
                        Actions = new[]
                        {
                            "s3:*"
                        },
                        Effect = Effect.ALLOW
                    })
                }
            });
            
            var user = new User(this, $"cdn-access-user-{env}");
            
            policy.AttachToUser(user);

            // var accessKey = new CfnAccessKey(this, $"cdn-access-user-key-{env}", new CfnAccessKeyProps
            // {
            //     UserName = user.UserName
            // });
            //
            // new StringParameter(this, $"cdn-access-user-accesskey-{env}", new StringParameterProps
            // {
            //     Type = ParameterType.SECURE_STRING,
            //     ParameterName = $"/{env}/cdn/accesskey",
            //     StringValue = accessKey.Ref,
            //     Tier = ParameterTier.STANDARD
            // });
            //
            // new StringParameter(this, $"cdn-access-user-secretaccesskey-{env}", new StringParameterProps
            // {
            //     Type = ParameterType.SECURE_STRING,
            //     ParameterName = $"/{env}/cdn/secretaccesskey",
            //     StringValue = accessKey.AttrSecretAccessKey,
            //     Tier = ParameterTier.STANDARD
            // });

            var assetsOrigin = new S3Origin(assetsBucket);
            var rootOrigin = new S3Origin(rootBucket);

            var assetsDistribution = new Distribution(this, $"cdn-assets-dist-{env}", new DistributionProps
            {
                DefaultBehavior = new BehaviorOptions
                {
                    AllowedMethods = AllowedMethods.ALLOW_GET_HEAD_OPTIONS,
                    Origin = assetsOrigin,
                    CachedMethods = CachedMethods.CACHE_GET_HEAD_OPTIONS,
                    Compress = true,
                    CachePolicy = new CachePolicy(this, $"cdn-assets-cache-policy-{env}", new CachePolicyProps
                    {
                        EnableAcceptEncodingBrotli = true,
                        EnableAcceptEncodingGzip = true
                    }),
                },
                Certificate = Certificate.FromCertificateArn(this, $"assets-certificate-{env}", props.ProjectEnvironment.cdn.assetsCertificateArn),
                DomainNames = new []{props.ProjectEnvironment.cdn.assetsDomainName},
                EnableLogging = false,
                PriceClass = PriceClass.PRICE_CLASS_ALL
            });

            var rootDistribution = new Distribution(this, $"cdn-root-dist-{env}", new DistributionProps
            {
                DefaultBehavior = new BehaviorOptions
                {
                    AllowedMethods = AllowedMethods.ALLOW_GET_HEAD_OPTIONS,
                    Origin = rootOrigin,
                    CachedMethods = CachedMethods.CACHE_GET_HEAD_OPTIONS,
                    Compress = true,
                    CachePolicy = new CachePolicy(this, $"cdn-root-cache-policy-{env}", new CachePolicyProps
                    {
                        EnableAcceptEncodingBrotli = true,
                        EnableAcceptEncodingGzip = true
                    })
                },
                Certificate = Certificate.FromCertificateArn(this, $"root-certificate-{env}", props.ProjectEnvironment.cdn.rootCertificateArn),
                DomainNames = new []{props.ProjectEnvironment.cdn.rootDomainName},
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

            ExportValue(assetsDistribution.DomainName, new ExportValueOptions
            {
                Name = $"assets-domain-name-{env}"
            });

            ExportValue(assetsDistribution.DistributionId, new ExportValueOptions
            {
                Name = $"assets-distribution-id-{env}"
            });

            ExportValue(assetsBucket.BucketName, new ExportValueOptions
            {
                Name = $"assets-bucket-name-{env}"
            });

            ExportValue(rootDistribution.DomainName, new ExportValueOptions
            {
                Name = $"root-domain-name-{env}"
            });

            ExportValue(rootDistribution.DistributionId, new ExportValueOptions
            {
                Name = $"root-distribution-id-{env}"
            });

            ExportValue(rootBucket.BucketName, new ExportValueOptions
            {
                Name = $"root-bucket-name-{env}"
            });

            StackParameter.AddStackParameter(this, "cdn", props);
        }
    }
}