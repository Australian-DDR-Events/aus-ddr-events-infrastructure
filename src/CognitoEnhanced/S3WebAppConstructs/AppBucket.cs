using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.S3;
using Constructs;

namespace CognitoEnhanced.S3WebAppConstructs
{
    public class AppBucket : Construct
    {
        public Bucket WebappBucket { get; private set; }
        
        public AppBucket(Construct scope, string id, AppBucketProps props = null) : base(scope, id)
        {
            WebappBucket = CreateAppBucket();
            var user = new User(this, "webapp-bucket-user");
            WebappBucket.GrantWrite(user);
        }

        private Bucket CreateAppBucket()
        {
            return new Bucket(this, "webapp-bucket", new BucketProps()
            {
                AccessControl = BucketAccessControl.PRIVATE,
                AutoDeleteObjects = false,
                BlockPublicAccess = BlockPublicAccess.BLOCK_ALL,
                PublicReadAccess = false
            });
        }
    }
}