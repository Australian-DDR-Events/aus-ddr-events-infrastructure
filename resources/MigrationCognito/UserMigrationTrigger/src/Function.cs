using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.DynamoDBv2;
using Firebase.Auth;
using Amazon.DynamoDBv2.Model;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MigrateUserTrigger
{
    public class Function
    {
        #nullable enable
        private FirebaseAuthProvider? provider;
        #nullable restore
        
        public async Task<CognitoEvent> FunctionHandler(CognitoEvent migrateUserEvent, ILambdaContext context)
        {
            Console.WriteLine($"Started auth check for user {migrateUserEvent.userName}");

            migrateUserEvent.response = migrateUserEvent.triggerSource switch
            {
                "UserMigration_Authentication" => await AuthenticationHandler(migrateUserEvent.userName, migrateUserEvent.request),
                "UserMigration_ForgotPassword" => await ForgotPasswordHandler(migrateUserEvent.userName),
                _ => migrateUserEvent.response
            };
            Console.WriteLine($"Finshed auth check for user {migrateUserEvent.userName}");
            return migrateUserEvent;
        }

        private async Task<MigrateUserResponse> AuthenticationHandler(string userName, MigrateUserRequest request)
        {

            if (provider == null)
            {
                provider = new FirebaseAuthProvider(new FirebaseConfig(Environment.GetEnvironmentVariable("FIREBASE_API_KEY")));
            }
            Console.WriteLine($"Starting sign in {userName}");
            try
            {
                var user = await provider.SignInWithEmailAndPasswordAsync(userName, request.password);
                Console.WriteLine($"Sign in complete for {userName}");
                if (user is {User: { }})
                {
                    return new MigrateUserResponse
                    {
                        userAttributes = new Dictionary<string, object>
                        {
                            {"email", user.User.Email},
                            {"username", user.User.Email},
                            {"email_verified", user.User.IsEmailVerified},
                            {"nickname", user.User.DisplayName},
                            {"custom:legacy_id", user.User.LocalId}
                        },
                        finalUserStatus = "CONFIRMED",
                        messageAction = "SUPPRESS",
                        desiredDeliveryMediums = new[] {"EMAIL"},
                        forceAliasCreation = false
                    };
                }
            }
            catch
            {
            }

            return null;
        }

        private async Task<MigrateUserResponse> ForgotPasswordHandler(string userName)
        {
            var config = new AmazonDynamoDBConfig();
            var client = new AmazonDynamoDBClient(config);

            var attribute = new AttributeValue(userName);
            var item = new Dictionary<string, AttributeValue>() {
                { "email", attribute}
            };
            
            var result = await client.GetItemAsync(Environment.GetEnvironmentVariable("USER_TABLE_NAME"), item);

            var email = result.Item["email"].S;
            var uid = result.Item["uid"].S;

            if (result.IsItemSet)
            {
                return new MigrateUserResponse
                {
                    userAttributes = new Dictionary<string, object>
                    {
                        {"email", email},
                        {"username", email},
                        {"email_verified", true},
                        {"nickname", "nothing"},
                        {"custom:legacy_id", uid}
                    },
                    finalUserStatus = "RESET_REQUIRED",
                    messageAction = "SUPPRESS",
                    desiredDeliveryMediums = new []{"EMAIL"},
                    forceAliasCreation = false
                };
            }

            return null;
        }
    }

    class User
    {
        public string email { get; set; }
        public string uid { get; set; }
    }
}
