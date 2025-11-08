FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# CORE
COPY  ["src/Core/Metriflow.Application/Metriflow.Application.csproj","src/Core/Metriflow.Application/"]
COPY  ["src/Core/Metriflow.Domain/Metriflow.Domain.csproj","src/Core/Metriflow.Domain/"]
# Execution
COPY ["src/Execution/Metriflow.API/Metriflow.API.csproj", "src/Execution/Metriflow.API/"]

# Infrastructure
COPY [ "src/Infrastructure/Metriflow.Infrastructure/Metriflow.Infrastructure.csproj","src/Infrastructure/Metriflow.Infrastructure/"]

WORKDIR /src  
COPY src/Core/ src/Core/ 
COPY src/Infrastructure/Metriflow.Infrastructure src/Infrastructure/Metriflow.Infrastructure
COPY src/Execution/Metriflow.API src/Execution/Metriflow.API

WORKDIR /src/src/Execution/Metriflow.API  

RUN dotnet restore "Metriflow.API.csproj"
RUN dotnet build "Metriflow.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Metriflow.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Metriflow.API.dll"]
