# Stage 1: Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the solution file and all project folders
COPY ["../../../Metriflow.sln", "."]
COPY ["src/", "src/"]

# Restore dependencies
RUN dotnet restore Metriflow.sln

# Navigate to the specific project and publish
WORKDIR /src/src/Metriflow.Correlation.Worker
RUN dotnet publish "Metriflow.Correlation.Worker.csproj" -c Release -o /app/publish

# Stage 2: Final Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copy published output from the build stage
COPY --from=build /app/publish .

# Set the entry point for the correlation worker
ENTRYPOINT ["dotnet", "Metriflow.Correlation.Worker.dll"]