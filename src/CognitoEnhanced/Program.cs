using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CognitoEnhanced
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            
            var env = (string) app.Node.TryGetContext("env");
            Console.WriteLine(env);
            var envFields = (Dictionary<string, object>)app.Node.TryGetContext(env);
            var appProps = AppStackProps.GetObject(envFields);
            appProps.Env = new Amazon.CDK.Environment() {
                Region = "ap-southeast-2",
            };

            new CognitoEnhancedStack(app, $"{env}-cognito-enhanced-stack", appProps);
            new S3WebApp(app, $"{env}-webapp-stack", appProps);
            new EC2App(app, $"{env}-ec2app-stack", appProps);
            app.Synth();
        }
    }
}
