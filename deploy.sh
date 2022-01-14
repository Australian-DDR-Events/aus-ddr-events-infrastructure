#/bin/bash

cdk bootstrap -c env="$env" --profile "$profile"

echo $env
echo $profile

cdk deploy -c env="$env" "$env-cognito-enhanced-stack" --profile "$profile"