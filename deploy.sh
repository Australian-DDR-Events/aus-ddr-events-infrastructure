#/bin/bash

cdk bootstrap -c env="$env"

echo $env

cdk deploy -c env="$env" "core/ecr-stack"

cdk deploy -c env="$env" "identity/cognito"

cdk deploy -c env="$env" "core/aus-ddr-events-cdn-$env-stack"
