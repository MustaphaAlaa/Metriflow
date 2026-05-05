# Metriflow: Web Analytics ETL and Reporting Platform

Metriflow is a high-throughput web analytics data platform designed to ingest large volumes of raw event data, process it through a multi-stage asynchronous aggregation pipeline, and expose secure reporting APIs for analytical consumption.

The system simulates external analytics sources such as Google Analytics (GA) and PageSpeed Insights (PSI), but the core focus of the project is backend architecture, throughput optimization, fault-tolerant processing, and scalable reporting.

<!-- In its current form, Metriflow is no longer a simple worker-based aggregator. It has evolved into a full ETL pipeline optimized for high-ingestion workloads, staged aggregation, and fast reporting across bounded and arbitrary date ranges. -->

## Table of Contents

- [Metriflow: Web Analytics ETL and Reporting Platform](#metriflow-web-analytics-etl-and-reporting-platform)
  - [Table of Contents](#table-of-contents)
  - [Project Overview](#project-overview)
  - [Key Features](#key-features)
    - [High-Throughput Data Ingestion](#high-throughput-data-ingestion)
    - [Queue-Based ETL Pipeline](#queue-based-etl-pipeline)
    - [Multi-Level Aggregation Strategy](#multi-level-aggregation-strategy)
    - [Late-Arriving and Duplicate Data Handling](#late-arriving-and-duplicate-data-handling)
    - [Secure Reporting API](#secure-reporting-api)
    - [Load Testing at Scale](#load-testing-at-scale)
    - [Containerized Development Environment](#containerized-development-environment)
    - [Why Both ADO.NET and EF Core?](#why-both-adonet-and-ef-core)
    - [Architectural Rationale](#architectural-rationale)
  - [Technology Stack](#technology-stack)
    - [Architecture Flow](#architecture-flow)
    - [Database Strategy](#database-strategy)
    - [Setup and Running the Application](#setup-and-running-the-application)
    - [Authentication](#authentication)
    - [Testing](#testing)
    - [CI/CD](#cicd)
    - [Current Engineering Focus](#current-engineering-focus)

## Project Overview

The platform processes analytics records through a staged pipeline:
Raw → Staged → Page → Intervals → Daily → Monthly → Yearly
Data ingestion and processing are deliberately decoupled through RabbitMQ. Multiple background workers execute aggregation sequentially while maintaining consistency in the presence of retries, duplicates, and late-arriving data.
The system has been benchmarked to process:
26 million records in under 3 minutes
(13M GA + 13M PSI processed in parallel)

**This was achieved through:**

- producer-consumer channels.
- streaming JSON deserialization
- batch bulk inserts using SqlBulkCopy
- minimized write contention on ingestion tables
- staged aggregation using stored procedures

---

## Key Features

### High-Throughput Data Ingestion

The ingestion layer reads structured analytics data from mock external sources and streams records without loading entire files into memory.

**Key implementation details:**

- streaming JSON deserialization for low memory pressure.
- producer-consumer channels for controlled parallelism.
- SqlBulkCopy batch persistence at 250,000 records per batch.

- parallel ingestion of GA and PSI datasets

This ingestion path is optimized for write throughput rather than ORM convenience.

---

### Queue-Based ETL Pipeline

RabbitMQ acts as the decoupling boundary between ingestion and downstream processing.

The system runs 7 background worker services, each responsible for a stage in the aggregation pipeline.

**Processing chain:**

- Raw ingestion.
- Staging normalization.
- Page-level aggregation.
- Interval aggregation.
- Daily aggregation.
- Monthly aggregation.
- Yearly aggregation.

This separation makes the pipeline easier to scale, retry, and reason about operationally.

---

### Multi-Level Aggregation Strategy

Aggregations are precomputed incrementally across bounded reporting dimensions.
Pre-aggregated tables support fast reporting across.

**common time slices:**

- page-level metrics.
- interval summaries.
- daily summaries.
- monthly summaries.
- yearly summaries.

For queries that do not fit these fixed aggregation boundaries, Redis is used to support arbitrary date-range lookups without creating unbounded combinations of precomputed tables.

---

### Late-Arriving and Duplicate Data Handling

The pipeline is built to maintain consistency when data arrives out of order or overlaps previously processed ranges.

**It does this by:**

- detecting existing aggregates.
- identifying upstream changes.
- recomputing affected downstream aggregates.
- updating partial results instead of blindly appending.

This ensures analytical correctness across the entire aggregation chain.

---

### Secure Reporting API

The reporting layer exposes REST endpoints for querying aggregated analytics.

**Security features include:**

- JWT authentication.
- ASP.NET Core Identity.
- Rate limiting for endpoint protection

<!-- The API is optimized for read-heavy reporting workloads. -->

---

### Load Testing at Scale

A companion load-testing tool was built specifically for this system.
Hyper JSON Generator produces millions of analytics records.

See [Hyper JSON Generator Code](https://github.com/MustaphaAlaa/HyperJSONGenerator)

**using:**

- Utf8JsonWriter
- CPU-cache-efficient structs

The tool exists to validate ingestion throughput and memory stability under large synthetic workloads.

---

### Containerized Development Environment

The full platform is containerized with Docker Compose.
Services include:

- API
- Worker services
- SQL Server
- RabbitMQ
- Redis

This ensures reproducible local development and predictable service orchestration.

---

### Why Both ADO.NET and EF Core?

A major architectural change in the current version is the deliberate use of both ADO.NET and Entity Framework Core.
This is not accidental and not transitional.
ADO.NET is used for write-heavy and throughput-critical workloads

**ADO.NET is used where raw performance matters most:**

- High-volume ingestion
- Batch writes via SqlBulkCopy
- Execution of aggregation stored procedures
- Explicit transaction handling
- Retry handling around transient failures

For these workloads, the abstraction cost of an ORM becomes material.

EF Core is used for read-heavy API query paths
EF Core remains valuable where maintainability and developer productivity matter more than raw write throughput.

**EF Core is used for:**

- Reporting queries
- API read models
- Identity/authentication persistence
- Ordinary relational access patterns
- Architectural Rationale

The project intentionally uses the right abstraction for the workload.

---

### Architectural Rationale

The project intentionally uses the right abstraction for the workload.

| Workload                      | Type Technology | Reason                                        |
| ----------------------------- | --------------- | --------------------------------------------- |
| High-throughput ingestion     | ADO.NET         | Lowest overhead, bulk insert support          |
| Complex aggregation execution | ADO.NET         | Stored procedures, transaction control        |
| Reporting reads               | EF Core         | Faster development, cleaner query composition |
| Identity/auth                 | EF Core         | Standard ASP.NET Core integration             |

This hybrid data-access model is one of the core technical decisions in the current architecture.

---

## Technology Stack

| Component                   | Technology                  | Role                                       |
| --------------------------- | --------------------------- | ------------------------------------------ |
| Backend API                 | ASP.NET Core (.NET 8)       | Secure reporting API                       |
| Worker Services             | .NET Worker Services        | Ingestion and staged aggregation           |
| Database                    | SQL Server                  | Persistent analytical storage              |
| High-throughput data access | ADO.NET                     | Bulk ingestion, stored procedure execution |
| ORM                         | Entity Framework Core       | Read-heavy query paths and identity        |
| Message Broker              | RabbitMQ                    | Asynchronous decoupled processing          |
| Cache                       | Redis                       | Arbitrary date-range reporting             |
| Authentication              | ASP.NET Core Identity + JWT | API security                               |
| Testing                     | xUnit, Moq                  | Aggregation and business logic validation  |
| CI/CD                       | GitHub Actions              | Build and validation pipeline              |
| Runtime                     | Docker Compose              | Local orchestration                        |

---

### Architecture Flow

**1. Ingestion.**

Analytics records from GA and PSI are streamed and inserted into raw ingestion tables using batched bulk operations.

**2. Queue Dispatch.**

RabbitMQ decouples ingestion from downstream aggregation.
Each stage consumes completed work from the previous stage.

**3. Sequential Aggregation Pipeline.**

Worker services execute staged transformations:
Raw → Staged → Page → Intervals → Daily → Monthly → Yearly
Aggregation logic is implemented primarily in stored procedures.

**4. Reporting API.**

The API reads pre-aggregated data using EF Core.
Redis supplements requests for non-standard date ranges.

---

### Database Strategy

Raw ingestion tables
Raw tables are optimized for writes.

**Design decisions include:**

- No clustered indexes on high-ingestion raw tables
- Minimized write contention during bulk insert operations

**Analytical tables**
Aggregated reporting tables use:

- Columnstore indexes

This provides:

- compression benefits

- improved scan speed

- better analytical query performance

**Aggregation execution**
Aggregation logic is encapsulated in stored procedures with:

- ACID transactions.

- Retry handling for transient failures.

**Database Schema Summary**
The exact schema may evolve, but the current model is conceptually organized around these layers:

| Table Group         | Purpose                                     |
| ------------------- | ------------------------------------------- |
| Raw tables          | High-throughput ingestion of source records |
| Staging tables      | Normalization and source merging            |
| Page aggregates     | Page-level correlation                      |
| Interval aggregates | Interval reporting                          |
| Daily aggregates    | Daily reporting                             |
| Monthly aggregates  | Monthly reporting                           |
| Yearly aggregates   | Yearly reporting                            |
| Identity tables     | Users, authentication, authorization        |

---

### Setup and Running the Application

**Prerequisites**
Install:

- Docker
- Docker Compose

Start the Platform
**From the repository root:**
`docker compose up --build -d`

This will start:

- API
- worker services
- SQL Server
- RabbitMQ
- Redis

Allow several moments for containers to initialize and for the initial pipeline execution to complete.

---

**Accessing the API**
Swagger should be available once the API container is ready.
Typical URL:
http://localhost:8080/swagger

---

### Authentication

Reporting endpoints are protected by JWT.
Typical flow:

Register a user

Log in

Copy returned JWT

Authorize through Swagger using:

` Bearer <token>`

Example Reporting Capabilities
Depending on the current API surface, reporting endpoints provide access to:

overall summaries

page-level analytics

bounded date-range reports

arbitrary date-range analytics (with Redis support when pre-aggregates are insufficient)

---

### Testing

The project includes unit tests covering core aggregation and business rules.
Run tests with:
dotnet test

---

### CI/CD

GitHub Actions is configured for automated validation.
Typical pipeline responsibilities include:

- restore
- build
- test execution

---

### Current Engineering Focus

The current version of Metriflow is primarily a backend systems engineering project focused on:

- Large-scale ingestion
- Asynchronous pipeline design
- Data consistency under reprocessing
- Hybrid data-access strategy

- Analytical query optimization

It is intentionally optimized around throughput, correctness, and operational scalability, rather than simple CRUD application design.
