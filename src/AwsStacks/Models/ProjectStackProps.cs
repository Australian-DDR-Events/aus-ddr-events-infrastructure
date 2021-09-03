using Amazon.CDK;

namespace AwsStacks.Models
{
    public class ProjectStackProps : StackProps
    {
        public ProjectEnvironment ProjectEnvironment { get; set; }
    }
}