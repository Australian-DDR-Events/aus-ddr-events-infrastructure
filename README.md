# Aus DDR Events Infrastructure

This hosts components to run the entire Aus DDR Events stack. This is broken down into a number of unique components, defined below.

Configuration for the stacks is done in the `cdk.json` file. In cases where manual configuration is required, it will be outlined.

Note these all require a number of SSL certs to be provided. These are manual steps that should be done prior to deployment. This _could_ be automated using ACM.

## Cognito Constructs

This is the identity provider used by both the site and the api. This can later be expanded to support other applications such as bots.

This has support for a user migraiton flow (bringing users across from firebase) as well as SES usage to send out emails.

## S3 WebApp Constructs

This hosts a SPA directly in an S3 bucket, backed by CloudFront as a reverse proxy. This has minimal configuration, however you will need to deploy your app separately.

## EC2 App Constructs

This hosts an application in an EC2 instance, backed by CloudFront as a reverse proxy. Due to limitations, this requires some manual configuration.

- You will need to setup an SSH key in advanced.
- SSH into the instance and edit the `ausddrapi` service -> `sudo nano /etc/systemd/system/ausddrapi.service`
- Change the `ASPNETCORE_ENVIRONMENT` to the desired environment.
- Following this, run both `sudo systemctl daemon-reload` and `sudo systemctl restart ausddrapi`
- The API will pull configuration down from SSM. These must be set beforehand.
