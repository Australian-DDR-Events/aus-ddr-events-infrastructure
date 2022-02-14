using System;
using System.Collections;
using System.Collections.Generic;
using Amazon.CDK;
using CognitoEnhanced.CognitoConstructs;
using CognitoEnhanced.EC2AppConstructs;
using CognitoEnhanced.S3CdnConstructs;
using CognitoEnhanced.S3WebAppConstructs;
using CloudFrontProxyProps = CognitoEnhanced.S3WebAppConstructs.CloudFrontProxyProps;

namespace CognitoEnhanced {
    public class AppStackProps : StackProps {
        public string label { get; set; }
        public MigrationCognitoProps migrationCognito { get; set; }
        public AppBucketProps appBucket { get; set; }
        public CloudFrontProxyProps cloudFrontProxy { get; set; }
        public CdnBucketProps cdnBucket { get; set; }
        public CdnCloudFrontProxyProps cdnProxy { get; set; }
        public HostedEC2AppProps hostedEC2AppProps { get; set; }
        public EC2AppCloudFrontProxyProps ec2AppCloudFrontProxyProps { get; set; }

        public static AppStackProps GetObject(Dictionary<string, object> dict)
        {
            var data = System.Text.Json.JsonSerializer.Serialize(dict);
            return System.Text.Json.JsonSerializer.Deserialize<AppStackProps>(data);
        }
    }
}
