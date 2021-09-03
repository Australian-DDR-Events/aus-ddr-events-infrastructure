using System.Collections.Generic;

#nullable enable

namespace MigrateUserTrigger
{
    public class MigrateUserResponse
    {
        public Dictionary<string, object>? userAttributes { get; set; }
        public string? finalUserStatus { get; set; }
        public string? messageAction { get; set; }
        public string[]? desiredDeliveryMediums { get; set; }
        public bool? forceAliasCreation { get; set; }
    }
}

#nullable restore