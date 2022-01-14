using System;

namespace MigrateUserTrigger
{
    public class CallerContext
    {
        public string awsSdkVersion { get; set; } = String.Empty;
        public string clientId { get; set; } = String.Empty;
    }
}