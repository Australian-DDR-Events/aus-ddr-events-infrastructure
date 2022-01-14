using System.Collections.Generic;
using Amazon.CDK.AWS.Cognito;

namespace CognitoEnhanced.CognitoConstructs {
    public class MigrationCognitoProps : UserPoolProps {
        public UserMigrationLambdaProps UserMigrationLambda { get; set; }
        
        public IList<UserClientDefinition> UserClients { get; set; }

        public IList<ResourceServerDefinition> ResourceServers { get; set; }
        
        public IList<UserPoolGroupDefinition> UserPoolGroups { get; set; }

        public string Domain { get; set; }
        public string SslArn { get; set; }
        
        public string SesArn { get; set; }
        public string EmailFromAddress { get; set; }
    }

    public class UserMigrationLambdaProps {
        public string ResourcesPath { get; set; }
        public string FirebaseApiKeyPath { get; set; }
    }

    public class UserClientDefinition
    {
        public string Identifier { get; set; }
        public List<string> CallbackUrls { get; set; }
        public List<string> LogoutUrls { get; set; }
        public bool UseBackend { get; set; }
        // public List<ClientResourceServerAssociation> Scopes { get; set; }
        // public List<string> DefaultScopes { get; set; }
    }
    
    public class ClientResourceServerAssociation {
        public string ResourceServerIdentifier { get; set; }
        public List<string> Scopes { get; set; }
    }

    public class ResourceServerDefinition {
        public string Identifier { get; set; }
        public IList<ResourceServerScopeDefinition> Scopes { get; set; } 
    }
    public class ResourceServerScopeDefinition {
        public string Description { get; set; }
        public string Name { get; set; }
    }

    public class UserPoolGroupDefinition
    {
        public string Description { get; set; }
        public string GroupName { get; set; }
    }
}