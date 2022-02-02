using Amazon.CDK;
using CognitoEnhanced.EC2AppConstructs;
using Constructs;

namespace CognitoEnhanced
{
    public class EC2App : Stack
    {
        public EC2App(Construct scope, string id, AppStackProps props) : base(scope, id, props)
        {
            var ec2App = new HostedEC2App(this, "hosted-ec2app", props.hostedEC2AppProps);
        }
    }
}