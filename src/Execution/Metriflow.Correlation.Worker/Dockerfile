# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY  ["src/Core/Metriflow.Application/Metriflow.Application.csproj","src/Core/Metriflow.Application/"]
COPY  ["src/Core/Metriflow.Domain/Metriflow.Domain.csproj","src/Core/Metriflow.Domain/"]
COPY ["src/Execution/Metriflow.Correlation.Worker/Metriflow.Correlation.Worker.csproj","src/Execution/Metriflow.Correlation.Worker/"]

# Copy rest of repository
# COPY ["src/", "src/"]
WORKDIR /src
COPY src/Core   src/Core  
COPY src/Execution/Metriflow.Correlation.Worker src/Execution/Metriflow.Correlation.Worker


# Publish from the project folder
WORKDIR /src/src/Execution/Metriflow.Correlation.Worker
RUN dotnet publish "Metriflow.Correlation.Worker.csproj" -c Release -o /app/publish

# Stage 2: Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output from the build stage
COPY --from=build /app/publish .

# Set the entry point for the correlation worker
ENTRYPOINT ["dotnet", "Metriflow.Correlation.Worker.dll"]