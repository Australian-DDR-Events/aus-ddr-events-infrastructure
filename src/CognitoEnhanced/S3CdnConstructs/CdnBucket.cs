using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using CognitoEnhanced.S3WebAppConstructs;
using Constructs;

namespace CognitoEnhanced.S3CdnConstructs
{
    public class CdnBucket : Construct
    {
        public Bucket Cdn { get; private set; }
        
        public CdnBucket(Construct scope, string id, CdnBucketProps props = null) : base(scope, id)
        {
            Cdn = CreateCdnBucket();
            var user = new User(this, "cdn-bucket-user");
            Cdn.GrantWrite(user);
        }

        private Bucket CreateCdnBucket()
        {
            return new Bucket(this, "cdn-bucket", new BucketProps()
            {
                AccessControl = BucketAccessControl.PRIVATE,
                AutoDeleteObjects = false,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                PublicReadAccess = false
            });
        }
    }
}