using System;
using System.Collections.Generic;

namespace AwsStacks.Models
{
    public class ProjectEnvironment
    {
        public string env { get; set; }
        public string resourcesPath { get; set; }
        public CognitoSubsection cognito { get; set; }
        
        public CdnSubsection cdn { get; set; }

        public static ProjectEnvironment GetObject(Dictionary<string, object> dict)
        {
            return (ProjectEnvironment) GetObject(dict, typeof(ProjectEnvironment));
        }

        private static object GetObject(Dictionary<string, object> dict, Type type)
        {
            var obj = Activator.CreateInstance(type);

            foreach (var kv in dict)
            {
                var prop = type.GetProperty(kv.Key);
                if (prop == null) continue;

                var value = kv.Value;
                if (value is Dictionary<string, object>)
                {
                    value = GetObject((Dictionary<string, object>) value, prop.PropertyType); // <= This line
                }

                prop.SetValue(obj, value, null);
            }

            return obj;
        }
    }

    public class CognitoSubsection
    {
        public string ssmPrefix { get; set; }
        public string sesArn { get; set; }
        public string emailFromAddress { get; set; }
        public string sslCertificateArn { get; set; }
        public string domain { get; set; }
    }

    public class CdnSubsection
    {
        public string assetsCertificateArn { get; set; }
        public string assetsDomainName { get; set; }
        public string rootCertificateArn { get; set; }
        public string rootDomainName { get; set; }
    }
}