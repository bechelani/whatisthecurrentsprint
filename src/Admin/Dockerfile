FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src

COPY src/Core/WhatIsTheCurrentSprint.Core.csproj Core/WhatIsTheCurrentSprint.Core.csproj
COPY src/Admin/WhatIsTheCurrentSprint.Admin.csproj Admin/WhatIsTheCurrentSprint.Admin.csproj

RUN dotnet restore "Admin/WhatIsTheCurrentSprint.Admin.csproj"

COPY src/Core/ Core/
COPY src/Admin/ Admin/

RUN dotnet build "Admin/WhatIsTheCurrentSprint.Admin.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Admin/WhatIsTheCurrentSprint.Admin.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WhatIsTheCurrentSprint.Admin.dll"]
