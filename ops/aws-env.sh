#/bin/bash

aws_profile=ausddrevents
env=staging

function update_ssm_entry()
{
    key=$1
    value=$2

    aws ssm put-parameter --name "$key" --value "$value" --overwrite --type String --tier Standard --profile $aws_profile
}

while read line; do
    IFS='=' read -r -a array <<< $line
    key="/$env/${array[0]}"
    value="${array[1]}"
    update_ssm_entry $key $value
done <aws-env.env
