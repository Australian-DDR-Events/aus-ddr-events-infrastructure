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
    key="/${env}/$( cut -d '=' -f 1 <<< "$line" )"
    value="$( cut -d '=' -f 2- <<< "$line" )"
    update_ssm_entry $key $value
done <aws-env.env
