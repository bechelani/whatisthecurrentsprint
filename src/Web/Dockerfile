FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src

COPY ./src/Core/WhatIsTheCurrentSprint.Core.csproj ./Core/WhatIsTheCurrentSprint.Core.csproj
COPY ./src/Web/WhatIsTheCurrentSprint.Web.csproj ./Web/WhatIsTheCurrentSprint.Web.csproj

RUN dotnet restore "./Web/WhatIsTheCurrentSprint.Web.csproj"

COPY ./src/Core/ ./Core/
COPY ./src/Web/ ./Web/

RUN dotnet build "./Web/WhatIsTheCurrentSprint.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./Web/WhatIsTheCurrentSprint.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WhatIsTheCurrentSprint.Web.dll"]
