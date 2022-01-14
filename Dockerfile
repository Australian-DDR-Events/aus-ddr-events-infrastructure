FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY resources_out /app/resources_out
COPY src /app/src
COPY cdk.json /app/cdk.json

RUN ls -la
RUN dotnet restore ./src/CognitoEnhanced.sln
RUN apt update -y
RUN apt install nodejs npm -y
RUN npm install -g npm@latest
RUN npm install -g aws-cdk

RUN cdk synthesize -c env="dev" --profile "dev" "dev-cognito-enhanced-stack" "dev-webapp-stack"