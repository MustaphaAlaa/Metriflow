# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# CORE
COPY  ["src/Core/Metriflow.Application/Metriflow.Application.csproj","src/Core/Metriflow.Application/"]
COPY  ["src/Core/Metriflow.Domain/Metriflow.Domain.csproj","src/Core/Metriflow.Domain/"]
# Infrastructure
COPY ["src/Infrastructure/Metriflow.Messages/Metriflow.Messages.csproj","src/Infrastructure/Metriflow.Messages/"]
# Execution
COPY ["src/Execution/Metriflow.Producer/Metriflow.Producer.csproj","src/Execution/Metriflow.Producer/"]

WORKDIR /src
COPY src/Execution/Metriflow.Producer src/Execution/Metriflow.Producer
COPY src/Core   src/Core 
COPY src/Infrastructure/Metriflow.Messages src/Infrastructure/Metriflow.Messages

# Navigate to the specific project and publish
WORKDIR /src/src/Execution/Metriflow.Producer
RUN dotnet publish "Metriflow.Producer.csproj" -c Release -o /app/publish

# Stage 2: Final Runtime Image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Copy published output from the build stage
COPY --from=build /app/publish .

# Set the entry point for the producer console app
ENTRYPOINT ["dotnet", "Metriflow.Producer.dll"] 
