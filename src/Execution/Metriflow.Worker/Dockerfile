# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# CORE
COPY  ["src/Core/Metriflow.Application/Metriflow.Application.csproj","src/Core/Metriflow.Application/"]
COPY  ["src/Core/Metriflow.Domain/Metriflow.Domain.csproj","src/Core/Metriflow.Domain/"]
# Execution
COPY ["src/Execution/Metriflow.Worker/Metriflow.Worker.csproj", "src/Execution/Metriflow.Worker/"]
COPY ["src/Execution/Metriflow.API/Metriflow.API.csproj", "src/Execution/Metriflow.API/"]
COPY ["src/Execution/Metriflow.Producer/Metriflow.Producer.csproj","src/Execution/Metriflow.Producer/"]
COPY ["src/Execution/Metriflow.Correlation.Worker/Metriflow.Correlation.Worker.csproj","src/Execution/Metriflow.Correlation.Worker/"]

# Infrastructure
COPY [ "src/Infrastructure/Metriflow.Infrastructure/Metriflow.Infrastructure.csproj","src/Infrastructure/Metriflow.Infrastructure/"]
COPY ["src/Infrastructure/Metriflow.Messaging/Metriflow.Messaging.csproj","src/Infrastructure/Metriflow.Messaging/"]


COPY ["src/", "src/"]

WORKDIR /src/src/Execution/Metriflow.Worker
RUN dotnet publish "Metriflow.Worker.csproj" -c Release -o /app/publish

# Stage 2: Final Runtime Image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Metriflow.Worker.dll"] 