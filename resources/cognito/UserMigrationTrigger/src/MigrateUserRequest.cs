using System.Collections.Generic;

namespace MigrateUserTrigger
{
    public class MigrateUserRequest
    {
        public string password { get; set; }
        public Dictionary<string, string> validationData { get; set; }
        public Dictionary<string, string> clientMetadata { get; set; }
    }
}