using System.Collections.Generic;
using Amazon.CDK.AWS.ECS;

namespace CognitoEnhanced.EC2AppConstructs
{
    public class HostedEC2AppProps : Ec2ServiceProps
    {
        public IList<string> AvailabilityZones { get; set; }
        public string SshKeyName { get; set; }
        public string UserDataPath { get; set; }
    }
}