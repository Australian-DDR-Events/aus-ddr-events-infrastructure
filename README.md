# AWS Stacks

This is a collection of AWS stacks used by AusDdrEvents. This will deploy the following stacks:

## identity/cognito

An AWS cognito UserPool. This is used for user and service authentication purposes. In addition, it will:
- Attach an SES for email purposes
- Register a firebase user migration trigger
- Add a custom domain with an ACM SSL certificate
- Create a PKCE test app

## core/ecr-stack

An ECR for storage of API docker images. This _probably_ needs to be made generic.

## core/aus-ddr-events-cdn-{ENVIRONMENT}-stack

A collection of resources for hosting a frontend in the form of two CDNs. This has one for `root` assets (such as the html and javascript), as well as general `assets`.

### root

- An S3 bucket for storage
- A cloudfront instance using the S3 bucket as an origin
- A custom domain using an ACM certificate
- A default file, `index.html`

### assets

- An S3 bucket for storage
- A cloudfront instance using the S3 bucket as an origin
- A custom domain using an ACM certificate

# Deploying

## Prerequisites

To follow this document, you will need to ensure you have [Docker](https://www.docker.com/) installed. This will take care of any other requirements.

**NOTE** no docker image yet for CDK. Need to update.

## Initial Configuration

You will need to create some resources in AWS prior to deployment. This is due to either:
- resources requiring initial validation
- resources that cannot be deployed using cdk

---
**NOTE**

The CDK stacks should be updated to support various configurations to avoid this setp for development purposes. For example, using the default Cognito email sender instead of SES, and using the default CloudFront domain instead of a custom domain.

Some of the resources will incur a fee. With little to no usage, this should be a small amount (less than $1 per month).

---

### Custom Domain

This expects that you have your own custom domain. I would recommend going for a provider like [Google](https://domains.google.com/registrar/), but anyone should work.

You will need the ability to add custom DNS records. Specifically:
- CNAME records
- TXT records
- MX records

### SES

SES (Simple Email Service) allows you to send emails from AWS. Create a new `Verified Identity` in the **US-EAST-1** region, and update your DNS settings with the DNS records provided.

After your identity has been verified, you will need to create a new `Email address identity` to test the service. This is because, by default, your SES account will be a sandbox account. To send emails to any address, you will need to leave the sandbox as outlined in [this document](https://docs.aws.amazon.com/console/ses/sandbox).

Be sure to take note of the ARN for your SES, it will be in the format:

`arn:aws:ses:us-east-1:{ACCOUNT}:identity/{DOMAIN}`


### ACM

ACM (Certificates Manager) is used to manage SSL certificates. You can import certificates from other providers such as [LetsEncrypt](https://letsencrypt.org/) or just ACM directly to generate your SSL certificates.

These should be stored in the **US-EAST-1** region. Click `Request a certificate` to start the process. You will need to request 3 separate certificates:
- Cognito certificate for identity purposes (eg. login.my-domain.com)
- A primary / root url certificate for the website (eg. my-domain.com)
- An assets url certificate for asset storage (eg. assets.my-domain.com)

You can alternatively add multiple domain names (in the form of additional names) to a single certificate if you wish.

After creation, you will need to add the DNS record provided. This is done to verify ownership. Shortly after applying DNS changes, the certificate will change from `Pending validation` to `Issued`.

Again, take note of the ARN for each of the certificates. They will be in the form:

`arn:aws:acm:us-east-1:{ACCOUNT}:certificate/{CERTIFICATE_ID}`

## Building Resources

Resources, otherwise referred to as assets, need to be built prior to deployment of the stacks. This can be done by running

`$ ./build_resources.sh` from the root directory.

This will result in a folder `resources_out` with your assets.

## Deploying CDK

Ensure you have logged into the AWS CLI tool. You can check this by running

`$ aws sts get-caller-identity`

Edit `cdk.json` to configure your stack. Under the `context` property, will be a `template` entry you can work off. Currently, all configuration fields are required. Please note the key used for your context will be required for all `cdk` commands. It is recommended you add this as an environment variable. All proceeding steps will assume you do so.

`$ export env=my-environment`

Lastly, deploy by running `deploy.sh` script

`$ ./deploy.sh`

This will create three CloudFormation stacks:
- A core ECR stack. This is for any docker images (to be used by the Aus DDR Events API).
- A cognito stack. This is for authentication. Comes with a migration trigger against firebase.
- A cdn stack. This has two CloudFront instances: one for the website hosting, and one for assets.


# Todo

### ACM Stack

If a domain uses Route53 as the DNS provider, CDK can automatically initiate an ACM request, update the DNS records and provision the certificate. [Documented here](https://docs.aws.amazon.com/cdk/api/latest/docs/aws-certificatemanager-readme.html)

### Create API stack

The API should be deployable to AWS. Unsure how this should be done.

- Using raw EC2. Cheapest option, quite a lot of manual configuration or scripts.
- Using ECS. Load balancer will add cost, can still be relatively cheap when using either EC2 or Fargate.
- Using EKS. Probably best option for scalability and ease of setup. Very costly.