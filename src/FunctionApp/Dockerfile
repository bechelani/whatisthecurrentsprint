# To enable ssh & remote debugging on app service change the base image to the one below
FROM mcr.microsoft.com/azure-functions/dotnet:3.0-appservice AS base
WORKDIR /home/site/wwwroot
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src

COPY ./src/FunctionApp/WhatIsTheCurrentSprint.FunctionApp.csproj ./FunctionApp/WhatIsTheCurrentSprint.FunctionApp.csproj

RUN dotnet restore "./FunctionApp/WhatIsTheCurrentSprint.FunctionApp.csproj"

COPY ./src/FunctionApp/ ./FunctionApp/

RUN dotnet build "./FunctionApp/WhatIsTheCurrentSprint.FunctionApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./FunctionApp/WhatIsTheCurrentSprint.FunctionApp.csproj" -c Release -o /app/publish

FROM base as final
WORKDIR /home/site/wwwroot
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true

COPY --from=publish /app/publish .
