using System;

#nullable enable

namespace MigrateUserTrigger
{
    public class CognitoEvent
    {
        public string version { get; set; } = String.Empty;
        public string triggerSource { get; set; } = String.Empty;
        public string region { get; set; } = String.Empty;
        public string userPoolId { get; set; } = String.Empty;
        public string userName { get; set; } = String.Empty;
        
        public CallerContext callerContext { get; set; }
        public MigrateUserRequest request { get; set; }
        public MigrateUserResponse? response { get; set; }
    }
}

#nullable restore