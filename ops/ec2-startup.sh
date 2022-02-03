yum update -y
rpm -Uvh https://packages.microsoft.com/config/centos/7/packages-microsoft-prod.rpm
yum install -y libicu60 gcc-c++

cd /home/ec2-user

wget https://download.visualstudio.microsoft.com/download/pr/d43345e2-f0d7-4866-b56e-419071f30ebe/68debcece0276e9b25a65ec5798cf07b/dotnet-sdk-6.0.101-linux-arm64.tar.gz
mkdir dotnet6
cd dotnet6
tar -xzf ../dotnet-sdk-6.0.101-linux-arm64.tar.gz

cd /home/ec2-user

wget https://github.com/unicode-org/icu/releases/download/release-70-1/icu4c-70_1-src.tgz
mkdir icubuild
cd icubuild
tar -xzf ../icu4c-70_1-src.tgz
cd icu/source

mkdir /home/ec2-user/libicu
./configure --prefix=/home/ec2-user/libicu
make -j 1
make install -j 1

cd /home/ec2-user

rm dotnet-sdk-6.0.101-linux-arm64.tar.gz
rm icu4c-70_1-src.tgz
rm -rf icubuild

sudo tee -a /etc/systemd/system/ausddrapi.service > /dev/null <<EOT
[Unit]
Description=Aus DDR Api Service
After=network.target
StartLimitIntervalSec=0

[Service]
Type=simple
Restart=always
RestartSec=15
User=ec2-user
ExecStart=/home/ec2-user/dotnet6/dotnet /home/ec2-user/aus-ddr-api/AusDdrApi.Api.dll
Environment=ASPNETCORE_ENVIRONMENT=staging
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000;https://0.0.0.0:5001

[Install]
WantedBy=multi-user.target
EOT

systemctl start ausddrapi
systemctl enable ausddrapi

test -x /usr/share/api && dotnet AusDdrApi.Api.dll