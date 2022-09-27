#/bin/bash

jq -s '.[0].context.'"$env"' += .[1] | .[0]' cdk_base.json ./cdk-environments/cdk_$env.json > cdk.json

cdk bootstrap -c env="$env" --profile "$profile"

echo $env
echo $profile

cdk deploy -c env="$env" "$env-cognito-enhanced-stack" --profile "$profile"