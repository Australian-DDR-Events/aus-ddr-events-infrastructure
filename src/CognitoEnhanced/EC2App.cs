using Amazon.CDK;
using CognitoEnhanced.EC2AppConstructs;
using Constructs;

namespace CognitoEnhanced
{
    public class EC2App : Stack
    {
        public HostedEC2App ec2app { get; }
        public EC2App(Construct scope, string id, AppStackProps props) : base(scope, id, props)
        {
            this.ec2app = new HostedEC2App(this, "hosted-ec2app", props.hostedEC2AppProps);
            new EC2AppCloudFrontProxy(this, "ec2app-cloudfront-proxy", this.ec2app.instance,
                props.ec2AppCloudFrontProxyProps);
        }
    }
}