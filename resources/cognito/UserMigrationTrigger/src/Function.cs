using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Firebase.Auth;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MigrateUserTrigger
{
    public class Function
    {
        private FirebaseAuthProvider? provider;
        
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
            var users = new List<User>()
            {

            };

            var u = users.FirstOrDefault(u => u.email.Equals(userName, StringComparison.OrdinalIgnoreCase));

            if (u != null)
            {
                return new MigrateUserResponse
                {
                    userAttributes = new Dictionary<string, object>
                    {
                        {"email", u.email},
                        {"username", u.email},
                        {"email_verified", true},
                        {"nickname", "nothing"},
                        {"custom:legacy_id", u.uid}
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
