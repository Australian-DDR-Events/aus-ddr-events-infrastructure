FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

ARG PROJECT
ENV PROJECT ${PROJECT}

RUN echo ${PROJECT}

# Copy everything else and build
COPY ${PROJECT}/src ./
RUN dotnet restore
RUN dotnet publish -c Release -o out
