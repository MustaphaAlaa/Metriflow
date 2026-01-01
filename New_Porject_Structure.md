# Metriflow Project File Layout

```
Metriflow/
│
├── 📄 CLEAN_ARCHITECTURE_ANALYSIS.md
├── 📄 docker-compose.yml
├── 📄 dotnet-tools.json
├── 📄 Metriflow.code-workspace
├── 📄 Metriflow.sln
├── 📄 Metriflow.sln.DotSettings.user
├── 📄 PROJECT_STRUCTURE.md
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
│   │   │   │   ├── 📁 Caches/
│   │   │   │   │   ├── 📄 IAnalyticsCacheServices.cs
│   │   │   │   │   └── 📄 ICacheService.cs
│   │   │   │   ├── 📁 MessageBrokers/
│   │   │   │   │   ├── 📄 IMessageBrokerConnection.cs
│   │   │   │   │   ├── 📄 IMessageBrokerConsumer.cs
│   │   │   │   │   ├── 📄 IMessageBrokerProducer.cs
│   │   │   │   │   └── 📄 IProducer.cs
│   │   │   │   └── 📁 Workers/
│   │   │   │       ├── 📄 IAnalyticRecordsCombiner.cs
│   │   │   │       ├── 📄 IAnalyticRecordsDeserializer.cs
│   │   │   │       ├── 📄 IKeyParser.cs
│   │   │   │       ├── 📄 IListsKeysServices.cs
│   │   │   │       ├── 📄 IRecordMatchingWorkflow.cs
│   │   │   │       ├── 📄 IRecordsMatcher.cs
│   │   │   │       └── 📄 IStreamData.cs
│   │   │   ├── 📁 Models/
│   │   │   │   └── 📁 Enums/
│   │   │   │       └── 📄 enCompletedListsNames.cs
│   │   │   └── 📁 Services/
│   │   │       ├── 📄 DailyStateCalculator.cs
│   │   │       ├── 📄 JsonSetting.cs
│   │   │       ├── 📄 PageServices.cs
│   │   │       ├── 📄 RawDataServices.cs
│   │   │       ├── 📁 Shared/
│   │   │       │   └── 📄 DateHelpers.cs
│   │   │       └── 📁 Workers/
│   │   │           ├── 📄 AnalyticRecordTypeResolver.cs
│   │   │           ├── 📄 KeyParser.cs
│   │   │           ├── 📄 ListsKeysServices.cs
│   │   │           ├── 📄 Producer.cs
│   │   │           ├── 📄 StreamData.cs
│   │   │           └── 📁 Matcher/
│   │   │               ├── 📄 AnalyticRecordsCombiner.cs
│   │   │               ├── 📄 RecordMatchingWorkflow.cs
│   │   │               └── 📄 RecordsMatcher.cs
│   │   │
│   │   └── 📁 Metriflow.Domain/
│   │       ├── 📄 Metriflow.Domain.csproj
│   │       ├── 📁 CustomAttributes/
│   │       │   └── 📄 AnalyticRecordAttribute.cs
│   │       ├── 📁 Entities/
│   │       │   ├── 📄 CombinedAnalyticsMessage.cs
│   │       │   ├── 📄 DailyStats.cs
│   │       │   ├── 📄 MonthlyStat.cs
│   │       │   ├── 📄 Page.cs
│   │       │   ├── 📄 RawData.cs
│   │       │   ├── 📄 User.cs
│   │       │   ├── 📄 YearlyStat.cs
│   │       │   ├── 📁 Enums/
│   │       │   │   └── 📄 enTypesKey.cs
│   │       │   ├── 📁 Reports/
│   │       │   │   ├── 📄 OverviewReport.cs
│   │       │   │   └── 📄 PageReport.cs
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
│   │   │   │   ├── 📄 IConsumerMessageHandler.cs
│   │   │   │   └── 📄 ICorrelationConsumer.cs
│   │   │   └── 📁 Services/
│   │   │       ├── 📄 ConsumerMessageHandler.cs
│   │   │       └── 📄 CorrelationConsumer.cs
│   │   │
│   │   ├── 📁 Metriflow.Matcher.Worker/
│   │   │   ├── 📄 Metriflow.Matcher.Worker.csproj
│   │   │   ├── 📄 Program.cs
│   │   │   ├── 📄 MatcherWorker.cs
│   │   │   ├── 📄 Dockerfile
│   │   │   ├── 📄 appsettings.json
│   │   │   ├── 📄 appsettings.Development.json
│   │   │   └── 📁 Properties/
│   │   │       └── 📄 launchSettings.json
│   │   │
│   │   └── 📁 Metriflow.Producer/
│   │       ├── 📄 Metriflow.Producer.csproj
│   │       ├── 📄 Program.cs
│   │       ├── 📄 MessageProducer.cs
│   │       ├── 📄 Dockerfile
│   │       ├── 📄 appsettings.json
│   │       └── 📁 data/
│   │           ├── 📄 GA-mock.json
│   │           └── 📄 PSI-mock.json
│   │
│   └── 📁 Infrastructure/
│       │
│       ├── 📁 Metriflow.Infrastructure/
│       │   ├── 📄 Metriflow.Infrastructure.csproj
│       │   ├── 📄 MetriflowDbContext.cs
│       │   ├── 📄 DbSets.cs
│       │   ├── 📄 InfrastructureLayerExtensions.cs
│       │   ├── 📁 EntitiesConfigurations/
│       │   │   ├── 📄 DailyStatConfiguration.cs
│       │   │   ├── 📄 MonthlyStatConfiguration.cs
│       │   │   ├── 📄 PageConfiguration.cs
│       │   │   ├── 📄 RawDataConfiguration.cs
│       │   │   ├── 📄 UserConfiguration.cs
│       │   │   └── 📄 YearlyStatConfiguration.cs
│       │   ├── 📁 Migrations/
│       │   │   ├── 📄 20251108201426_Initial.cs
│       │   │   ├── 📄 20251108201426_Initial.Designer.cs
│       │   │   └── 📄 MetriflowDbContextModelSnapshot.cs
│       │   └── 📁 Repositories/
│       │       ├── 📄 BaseRepository.cs
│       │       ├── 📄 DailyStatRepository.cs
│       │       ├── 📄 PageRepository.cs
│       │       └── 📄 UnitOfWork.cs
│       │
│       ├── 📁 Metriflow.Messages/
│       │   ├── 📄 Metriflow.Messages.csproj
│       │   ├── 📁 Connections/
│       │   │   └── 📄 RabbitMqConnection.cs
│       │   ├── 📁 Consumers/
│       │   │   └── 📄 RabbitMqConsumer.cs
│       │   ├── 📁 Extensions/
│       │   │   └── 📄 MessageDiExtensions.cs
│       │   └── 📁 Producers/
│       │       └── 📄 RabbitMqProducer.cs
│       │
│       └── 📁 Redis/
│           ├── 📄 Redis.csproj
│           ├── 📄 RedisServices.cs
│           ├── 📄 RedisAnalyticRecordDeserializer.cs
│           ├── 📄 RedisCompletedAnalyticsStore.cs
│           └── 📁 Extensions/
│               └── 📄 RedisDiExtensions.cs
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

- **Metriflow.Application**: Application services, business logic, interfaces, and worker implementations
  - Extensions for dependency injection
  - Interfaces organized by concern (Caches, MessageBrokers, Workers)
  - Services including calculators, page services, and raw data services
  - Worker services for matching, combining, and processing analytics records
  - Models and enums for application-level types

- **Metriflow.Domain**: Domain entities, DTOs, interfaces, and repository contracts
  - Custom attributes for domain modeling
  - Entities including stats, pages, users, and analytics messages
  - Report entities for page and overview reports
  - Worker-related entities (GA/PSI records, RabbitMQ settings)
  - Repository interfaces and unit of work pattern

### **Infrastructure Layer** (`src/Infrastructure/`)

- **Metriflow.Infrastructure**: Database context, migrations, entity configurations, and repository implementations
  - EF Core DbContext and DbSets
  - Entity configurations for all domain entities
  - Database migrations
  - Repository implementations and UnitOfWork

- **Metriflow.Messages**: Message broker infrastructure (RabbitMQ)
  - Connection management
  - Consumer and producer implementations
  - Dependency injection extensions

- **Redis**: Redis caching infrastructure
  - Redis services and stores
  - Analytic record deserialization
  - Dependency injection extensions

### **Execution Layer** (`src/Execution/`)

- **Metriflow.API**: REST API application
- **Metriflow.AggregationWorker**: Background worker for data aggregation
- **Metriflow.Correlation.Worker**: Worker for correlation processing
- **Metriflow.Matcher.Worker**: Worker for record matching
- **Metriflow.Producer**: Message producer service with mock data

### **Tests** (`tests/`)

- Unit test projects for major components:
  - AggregationWorker tests
  - API tests
  - Application tests
  - Producer tests (with test files for message producer, producer, and stream data)

---

_Note: `bin/` and `obj/` folders (build outputs) are not shown in this structure as they are typically ignored in source control._

