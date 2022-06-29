#/bin/bash

api_key=
owner_guid=
env=staging

function get_current_circleci_id()
{
    env_id_query=".items | .[] | select(.name==\"$env\") | .id"
    env_id=$(curl -s --location --request GET "https://circleci.com/api/v2/context?owner-id=$owner_guid" \
    --header "Circle-Token: $api_key" | jq "$env_id_query" | tr -d '"')

    echo $env_id
}

function create_new_circleci_context()
{
    data='{
        "name": '"\"$env\""',
        "owner": {
            "id": '"\"$owner_guid\""',
            "type": "organization"
        }
    }'
    query=".id"
    env_id=$(curl -s --location --request POST "https://circleci.com/api/v2/context" \
    --header "Circle-Token: $api_key" \
    --header 'Content-Type: application/json' \
    --data-raw ''"$data"'' | jq "$query" | tr -d '"')
    echo $env_id
}

function set_context_value
{
    context=$1
    name=$2
    value=$3
    # echo $context $name $value "\n"

    data='{"value": '"\"$value\""'}'
    # echo $data
    curl -s --location --request PUT "https://circleci.com/api/v2/context/$context/environment-variable/$name" \
    --header "Circle-Token: $api_key" \
    --header 'Content-Type: application/json' \
    --data-raw ''"$data"''
}

id=$(get_current_circleci_id)
[[ -z $id ]] && id=$(create_new_circleci_context)

# echo $id
while read line; do
    # array=(${line//=/ })
    IFS='=' read -r -a array <<< $line
    echo "Updating ${array[0]}\n"
    set_context_value $id ${array[0]} ${array[1]}
done <circleci-env.env