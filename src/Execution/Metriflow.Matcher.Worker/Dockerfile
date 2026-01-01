FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Execution/Matcher.Worker/Matcher.Worker.csproj", "src/Execution/Matcher.Worker/"]
RUN dotnet restore "src/Execution/Matcher.Worker/Matcher.Worker.csproj"
COPY . .
WORKDIR "/src/src/Execution/Matcher.Worker"
RUN dotnet build "./Matcher.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Matcher.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Matcher.Worker.dll"]
