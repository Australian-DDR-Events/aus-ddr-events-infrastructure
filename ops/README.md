## circleci-env.env
This file holds environment variables required by CircleCI. They should be populated according to the below definitions.
`circleci-env.sh` should be appropriately populated with your CircleCI token and organisation id.

```
API_BASE - The base URL for a hosted API. eg. api.ausddrevents.com
API_HOST - The IP address for the API's ec2 instance. eg. 54.79.220.150
API_URL - The full URL for a hosted API. eg. https://api.ausddrevents.com
ASSETS_URL - The base URL for hosted assets. eg. assets.ausddrevents.com
AUDIENCE - An IDP audience to be used for calling the API. eg. AusDdrEventsApi
AWS_ACCESS_KEY - An AWS Access Key. Should be configured for sufficient access.
AWS_REGION - The AWS region to host infrastructure. eg. ap-southeast-2
AWS_SECRET_ACCESS_KEY - An AWS Secret Access Key. Should be associated with the Access Key.
CLIENT_ID - An IDP client id to be used for login. Should be setup with correct urls. eg. 47hi38kgs1odrpcin4i54de927
EC2_SSH_KEY - A pre-configured SSH key to be used for accessing the ec2 instance. Newline characters should be replaced with \n
ENVIRONMENT - The environment identifier to be used by CircleCI. eg. prod
PROVIDER - The desired IDP base URL. eg. id.ausddrevents.com
WEBAPP_BUCKET_EXPORT - The name of the webapp S3 bucket. eg. Exportprodbucketname
WEBAPP_PROXY_EXPORT - The name of the webapp CloudFront proxy. eg. Exportprodproxyid
```

