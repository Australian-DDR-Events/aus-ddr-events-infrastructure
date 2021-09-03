#/bin/bash

function build_stack_lambdas()
{
    stack_name=$1
    for dir in $(find ./resources/$stack_name -maxdepth 1 -type d ! -path "./resources/$stack_name")
    do
        dir=${dir#"./resources/"}
        docker build --build-arg PROJECT=$dir -t tmp ./resources
        docker container create --name temp tmp

        mkdir -p ./resources_out/$dir
        #zip -r ./resources_out/$dir/out.zip temp:/app/out/.
        docker container cp -a temp:/app/out/. ./resources_out/$dir/out
        docker container rm temp
        docker rmi tmp
        zip -j -r ./resources_out/$dir/out.zip ./resources_out/$dir/out
        rm -r ./resources_out/$dir/out
        echo $dir
    done
}

mkdir -p resources_out

stacks=("cognito")

for stack in ${stacks[@]}; do
  build_stack_lambdas $stack
done