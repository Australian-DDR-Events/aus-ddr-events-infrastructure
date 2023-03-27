using System.IO;
using System.Linq;
using System.Text;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.IAM;
using Constructs;

namespace CognitoEnhanced.EC2AppConstructs
{
    public class HostedEC2App : Construct
    {
        public Instance_ instance { get; private set; }
        public Role iamRole { get; private set; }
        public HostedEC2App(Construct scope, string id, HostedEC2AppProps props) : base(scope, id)
        {
            var vpc = CreateVpc();
            var securityGroup = CreateSecurityGroup(vpc);
            this.iamRole = CreateIamRole();
            this.instance = CreateInstance(props, vpc, securityGroup, this.iamRole);
            
            var userData = File.ReadAllText(props.UserDataPath, Encoding.UTF8);
            this.instance.AddUserData(userData);
        }

        private Vpc CreateVpc()
        {
            var vpc = new Vpc(this, "ec2app-vpc", new VpcProps
            {
                IpAddresses = IpAddresses.Cidr("10.0.0.0/16"),
                NatGateways = 0,
                SubnetConfiguration = new ISubnetConfiguration[]
                {
                    new SubnetConfiguration
                    {
                        Name = "public",
                        CidrMask = 24,
                        SubnetType = SubnetType.PUBLIC
                    }
                }
            });

            return vpc;
        }

        private SecurityGroup CreateSecurityGroup(Vpc vpc)
        {
            var securityGroup = new SecurityGroup(this, "ec2app-securitygroup", new SecurityGroupProps
            {
                Vpc = vpc,
                AllowAllOutbound = true
            });
            
            securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(22), "SSH for ipv4 addresses");
            securityGroup.AddIngressRule(Peer.AnyIpv6(), Port.Tcp(22), "SSH for ipv6 addresses");
            
            securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(80), "HTTP traffic for ipv4 addresses");
            securityGroup.AddIngressRule(Peer.AnyIpv6(), Port.Tcp(80), "HTTP traffic for ipv6 addresses");
            
            securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(443), "HTTPS traffic for ipv4 addresses");
            securityGroup.AddIngressRule(Peer.AnyIpv6(), Port.Tcp(443), "HTTPS traffic for ipv6 addresses");
            
            securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(5000), "HTTP API traffic for ipv4 addresses");
            securityGroup.AddIngressRule(Peer.AnyIpv6(), Port.Tcp(5000), "HTTP API traffic for ipv6 addresses");
            
            securityGroup.AddIngressRule(Peer.AnyIpv4(), Port.Tcp(5001), "HTTPS API traffic for ipv4 addresses");
            securityGroup.AddIngressRule(Peer.AnyIpv6(), Port.Tcp(5001), "HTTPS API traffic for ipv6 addresses");

            return securityGroup;
        }

        private Role CreateIamRole()
        {
            return new Role(this, "ec2app-role", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("AmazonSSMManagedInstanceCore"),
                    ManagedPolicy.FromAwsManagedPolicyName("AmazonSSMReadOnlyAccess"), 
                }
            });
        }

        private Instance_ CreateInstance(HostedEC2AppProps props, Vpc vpc, SecurityGroup securityGroup, Role role)
        {
            var image = MachineImage.LatestAmazonLinux(new AmazonLinuxImageProps
            {
                Generation = AmazonLinuxGeneration.AMAZON_LINUX_2,
                CpuType = AmazonLinuxCpuType.ARM_64,
                Edition = AmazonLinuxEdition.STANDARD,
                Storage = AmazonLinuxStorage.GENERAL_PURPOSE,
            });

            return new Instance_(this, "ec2app-instance", new InstanceProps
            {
                MachineImage = image,
                InstanceType = new InstanceType("t4g.nano"),
                Vpc = vpc,
                VpcSubnets = new SubnetSelection
                {
                    SubnetType = SubnetType.PUBLIC
                },
                SecurityGroup = securityGroup,
                Role = role,
                KeyName = props.SshKeyName
            });
        }
    }
}