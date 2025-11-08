# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# CORE
COPY  ["src/Core/Metriflow.Application/Metriflow.Application.csproj","src/Core/Metriflow.Application/"]
COPY  ["src/Core/Metriflow.Domain/Metriflow.Domain.csproj","src/Core/Metriflow.Domain/"]
# Execution
COPY ["src/Execution/Metriflow.AggregationWorker/Metriflow.AggregationWorker.csproj", "src/Execution/Metriflow.AggregationWorker/"] 

# Infrastructure
COPY [ "src/Infrastructure/Metriflow.Infrastructure/Metriflow.Infrastructure.csproj","src/Infrastructure/Metriflow.Infrastructure/"]
COPY ["src/Infrastructure/Metriflow.Messaging/Metriflow.Messaging.csproj","src/Infrastructure/Metriflow.Messaging/"]


COPY ["src/", "src/"]

WORKDIR /src/src/Execution/Metriflow.AggregationWorker
RUN dotnet publish "Metriflow.AggregationWorker.csproj" -c Release -o /app/publish

# Stage 2: Final Runtime Image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Metriflow.AggregationWorker.dll"] 