# Metriflow Project Structure

## Old Project Structure

> This structure implementation violating Clean architecture and some file violate SOLID Principles.

```
Metriflow/
│
├── 📄 docker-compose.yml
├── 📄 dotnet-tools.json
├── 📄 Metriflow.code-workspace
├── 📄 Metriflow.sln
├── 📄 Metriflow.sln.DotSettings.user
├── 📄 README.md
│
├── 📁 src/
│   │
│   ├── 📁 Core/
│   │   │
│   │   ├── 📁 Metriflow.Application/
│   │   │   ├── 📄 Metriflow.Application.csproj
│   │   │   ├── 📁 Extensions/
│   │   │   │   └── 📄 ApplicationLayerExtensions.cs
│   │   │   ├── 📁 Interfaces/
│   │   │   │   ├── 📄 IDailyStateCalculator.cs
│   │   │   │   ├── 📄 IPageServices.cs
│   │   │   │   ├── 📄 IRawDataServices.cs
│   │   │   │   └── 📁 Wokrers/
│   │   │   │       ├── 📄 IRabbitMQConnection.cs
│   │   │   │       ├── 📄 IRabbitMQConsumer.cs
│   │   │   │       └── 📄 IRabbitMQProducer.cs
│   │   │   └── 📁 Services/
│   │   │       ├── 📄 DailyStateCalculator.cs
│   │   │       ├── 📄 JsonSetting.cs
│   │   │       ├── 📄 PageServices.cs
│   │   │       ├── 📄 RawDataServices.cs
│   │   │       ├── 📁 Shared/
│   │   │       │   └── 📄 DateHelpers.cs
│   │   │       └── 📁 Workers/
│   │   │           ├── 📁 Connections/
│   │   │           │   └── 📄 RabbitMQConnection.cs
│   │   │           ├── 📁 Consumers/
│   │   │           │   └── 📄 RabbitMQConsumer.cs
│   │   │           └── 📁 Producers/
│   │   │               └── 📄 RabbitMQProducer.cs
│   │   │
│   │   └── 📁 Metriflow.Domain/
│   │       ├── 📄 Metriflow.Domain.csproj
│   │       ├── 📁 CustomAttributes/
│   │       │   └── 📄 AnalyticRecordAttribute.cs
│   │       ├── 📁 DTO/
│   │       │   ├── 📄 CombinedAnalyticsMessage.cs
│   │       │   ├── 📄 OverviewReportDto.cs
│   │       │   └── 📄 PageReportDto.cs
│   │       ├── 📁 Enities/
│   │       │   ├── 📄 DailyStats.cs
│   │       │   ├── 📄 MonthlyStat.cs
│   │       │   ├── 📄 Page.cs
│   │       │   ├── 📄 RawData.cs
│   │       │   ├── 📄 User.cs
│   │       │   ├── 📄 YearlyStat.cs
│   │       │   └── 📁 Workers/
│   │       │       ├── 📄 enPages.cs
│   │       │       ├── 📄 GARecord.cs
│   │       │       ├── 📄 PSIRecord.cs
│   │       │       └── 📄 RabbitMqSettings.cs
│   │       ├── 📁 Interfaces/
│   │       │   ├── 📄 IAnalyticRecord.cs
│   │       │   └── 📄 IPageStats.cs
│   │       └── 📁 IRepository/
│   │           ├── 📄 IBaseRepository.cs
│   │           ├── 📄 IDailySateRepository.cs
│   │           ├── 📄 IPageRepository.cs
│   │           └── 📄 IUnitOfWork.cs
│   │
│   ├── 📁 Execution/
│   │   │
│   │   ├── 📁 Metriflow.AggregationWorker/
│   │   │   ├── 📄 Metriflow.AggregationWorker.csproj
│   │   │   ├── 📄 Program.cs
│   │   │   ├── 📄 Worker.cs
│   │   │   ├── 📄 Dockerfile
│   │   │   ├── 📄 appsettings.json
│   │   │   ├── 📄 appsettings.Development.json
│   │   │   ├── 📁 Properties/
│   │   │   │   └── 📄 launchSettings.json
│   │   │   ├── 📁 Interfaces/
│   │   │   │   ├── 📄 IAggregationConsumer.cs
│   │   │   │   ├── 📄 IDailyStatOrchestrator.cs
│   │   │   │   └── 📄 IRawDataIngestionOrchestrator.cs
│   │   │   └── 📁 Services/
│   │   │       ├── 📄 AggregationConsumer.cs
│   │   │       ├── 📄 DailyStatOrchestrator.cs
│   │   │       ├── 📄 RawDataIngestionOrchestrator.cs
│   │   │       └── 📄 WorkerConsumer.cs
│   │   │
│   │   ├── 📁 Metriflow.API/
│   │   │   ├── 📄 Metriflow.API.csproj
│   │   │   ├── 📄 Program.cs
│   │   │   ├── 📄 Dockerfile
│   │   │   ├── 📄 Metriflow.API.http
│   │   │   ├── 📄 appsettings.json
│   │   │   ├── 📄 appsettings.Development.json
│   │   │   └── 📁 Properties/
│   │   │       └── 📄 launchSettings.json
│   │   │
│   │   ├── 📁 Metriflow.Correlation.Worker/
│   │   │   ├── 📄 Metriflow.Correlation.Worker.csproj
│   │   │   ├── 📄 Program.cs
│   │   │   ├── 📄 CorrelationWorker.cs
│   │   │   ├── 📄 Dockerfile
│   │   │   ├── 📄 appsettings.json
│   │   │   ├── 📄 appsettings.Development.json
│   │   │   ├── 📁 Properties/
│   │   │   │   └── 📄 launchSettings.json
│   │   │   ├── 📁 Interfaces/
│   │   │   │   ├── 📄 enRedisListsNames.cs
│   │   │   │   ├── 📄 IConsumerMessageHandler.cs
│   │   │   │   ├── 📄 ICorrelationConsumer.cs
│   │   │   │   ├── 📄 ICorrelationRedisQueries.cs
│   │   │   │   ├── 📄 IProducer.cs
│   │   │   │   └── 📄 IRecordsMatcher.cs
│   │   │   └── 📁 Services/
│   │   │       ├── 📄 AnalyticRecordsCombiner.cs
│   │   │       ├── 📄 ConsumerMessageHandler.cs
│   │   │       ├── 📄 CorrelationConsumer.cs
│   │   │       ├── 📄 CorrelationRedisQueries.cs
│   │   │       ├── 📄 Helpers.cs
│   │   │       ├── 📄 RawDataProducer.cs
│   │   │       └── 📄 RecordsMatcher.cs
│   │   │
│   │   └── 📁 Metriflow.Producer/
│   │       ├── 📄 Metriflow.Producer.csproj
│   │       ├── 📄 Program.cs
│   │       ├── 📄 MessageProducer.cs
│   │       ├── 📄 Dockerfile
│   │       ├── 📄 appsettings.json
│   │       ├── 📁 data/
│   │       ├── 📁 Interface/
│   │       │   ├── 📄 IProducer.cs
│   │       │   └── 📄 IStreamData.cs
│   │       └── 📁 Services/
│   │           ├── 📄 Producer.cs
│   │           └── 📄 StreamData.cs
│   │
│   └── 📁 Infrastructure/
│       │
│       └── 📁 Metriflow.Infrastructure/
│           ├── 📄 Metriflow.Infrastructure.csproj
│           ├── 📄 MetriflowDbContext.cs
│           ├── 📄 DbSets.cs
│           ├── 📄 InfrastructureLayerExtensions.cs
│           ├── 📁 EntitiesConfigurations/
│           │   ├── 📄 DailyStatConfiguration.cs
│           │   ├── 📄 MonthlyStatConfiguration.cs
│           │   ├── 📄 PageConfiguration.cs
│           │   ├── 📄 RawDataConfiguration.cs
│           │   ├── 📄 UserConfiguration.cs
│           │   └── 📄 YearlyStatConfiguration.cs
│           ├── 📁 Migrations/
│           │   ├── 📄 20251108201426_Initial.cs
│           │   ├── 📄 20251108201426_Initial.Designer.cs
│           │   └── 📄 MetriflowDbContextModelSnapshot.cs
│           └── 📁 Repositories/
│               ├── 📄 BaseRepository.cs
│               ├── 📄 DailyStatRepository.cs
│               ├── 📄 PageRepository.cs
│               └── 📄 UnitOfWork.cs
│
└── 📁 tests/
    │
    ├── 📁 Metriflow.AggregationWorker.UnitTests/
    │   └── 📄 Metriflow.AggregationWorker.UnitTests.csproj
    │
    ├── 📁 Metriflow.API.UnitTests/
    │   └── 📄 Metriflow.API.UnitTests.csproj
    │
    ├── 📁 Metriflow.Application.UnitTests/
    │   └── 📄 Metriflow.Application.UnitTests.csproj
    │
    └── 📁 Metriflow.Producers.UnitTests/
        ├── 📄 Metriflow.Producers.UnitTests.csproj
        ├── 📄 MessageProducerTests.cs
        ├── 📄 ProducerTests.cs
        └── 📄 StreamDataTests.cs
```

## Project Organization Summary

### **Core Layer** (`src/Core/`)

- **Metriflow.Application**: Application services and business logic
- **Metriflow.Domain**: Domain entities, DTOs, interfaces, and repositories

### **Infrastructure Layer** (`src/Infrastructure/`)

- **Metriflow.Infrastructure**: Database context, migrations, entity configurations, and repository implementations

### **Execution Layer** (`src/Execution/`)

- **Metriflow.API**: REST API application
- **Metriflow.AggregationWorker**: Background worker for data aggregation
- **Metriflow.Correlation.Worker**: Worker for correlation processing
- **Metriflow.Producer**: Message producer service

### **Tests** (`tests/`)

- Unit test projects for each major component

---

_Note: `bin/` and `obj/` folders (build outputs) are not shown in this structure as they are typically ignored in source control._
