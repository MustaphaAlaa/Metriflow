# **Web Analytics Data Aggregator (ElectroPi Quest)**

This repository contains the solution for the ElectroPi "Web Analytics Data Aggregator" quest. The system is designed to ingest raw data from mocked external web analytics sources (Google Analytics and PageSpeed Insights), process it through a real message broker (RabbitMQ), aggregate daily statistics, and expose secure reporting APIs.

## **🚀 Key Features**

- **Data Ingestion:** Reads and combines raw JSON data from two sources (GA and PSI) into a standardized analytical record.
- **Real-Time Queuing (RabbitMQ):** Uses a dedicated **.NET Worker Service (Producer)** to publish raw records to a RabbitMQ queue for reliable asynchronous processing.
- **Asynchronous Aggregation:** A **.NET Worker Service (Consumer)** consumes records, calculates daily totals (users, sessions, views) and averages (performance score), and persists the aggregated data to a SQL Server database via EF Core.
- **Reliability:** Implements basic message retry logic (3 attempts with backoff) and guarantees message acknowledgment only upon successful database save.
- **Secure Reporting API (ASP.NET Core):** Exposes aggregated data via JWT-protected endpoints.
- **Containerization:** Full setup managed via docker-compose.yml, including the API, Database, and RabbitMQ Broker.

## **🛠️ Tech Stack**

| Component             | Technology                      | Role                                                              |
| :-------------------- | :------------------------------ | :---------------------------------------------------------------- |
| **Backend API**       | ASP.NET Core (.NET 8\)          | RESTful Reporting API                                             |
| **Producer/Consumer** | .NET Worker Service             | Handles Ingestion, Queuing, and Aggregation                       |
| **Database**          | SQL Server (via Docker)         | Persistent storage for Raw Data and Daily Statistics              |
| **ORM**               | Entity Framework Core (EF Core) | Database interaction and migrations                               |
| **Message Broker**    | RabbitMQ                        | Reliable, asynchronous messaging                                  |
| **Authentication**    | JWT (Bearer Token)              | Securing API endpoints                                            |
| **Caching**           | Redis                           | Used for session management or potential rate limiting (optional) |
| **Runtime**           | Docker Compose                  | Orchestration of all services                                     |

## **📐 Architecture Flow Diagram**

The system operates based on a clear, unidirectional flow, ensuring data processing is decoupled and reliable.

1. **Producer Service:** Reads mock GA/PSI JSON files upon startup.
2. **Publishing:** Combines raw records and publishes them to the analytics.raw exchange on **RabbitMQ**.
3. **Consumption:** The **Consumer Service** is bound to the analytics.raw.q queue and receives messages.
4. **Aggregation:** The Consumer calculates daily and page-level aggregates.
5. **Persistence:** Aggregated data is saved/updated in the **SQL Server DB** (DailyStats table) using EF Core.
6. **Reporting:** The **ASP.NET API** serves the final aggregated data from the DB via JWT-protected endpoints.

## **⚙️ Setup and Running the Application**

### **Prerequisites**

You must have the following installed:

- [Docker](https://www.docker.com/get-started/)
- Docker Compose (Usually included with Docker Desktop)

### **Step 1: Start All Services**

Navigate to the root directory of the repository where the docker-compose.yml file is located and run:

docker compose up \--build \-d

This command will:

1. Build the ASP.NET Core API and Worker Service images.
2. Start the RabbitMQ broker, SQL Server database, and Redis cache containers.
3. Run the **Producer Worker Service**, which will automatically read the bundled mock JSON files and start publishing data to RabbitMQ.
4. Run the **Consumer Worker Service**, which will start listening to the queue, aggregating data, and persisting it to the SQL Server DB.

Wait a few moments for the SQL Server to initialize and the workers to process the initial data queue.

### **Step 2: Access Swagger and Seed/Login**

The API is available at port 8080\.

1. Open the Swagger UI in your browser: http://localhost:8080/swagger/index.html

### **Step 3: Obtain a JSON Web Token (JWT)**

All reporting endpoints are secured using JWT Bearer Authentication.

1. **Sign Up:** Use the /Auth/signup endpoint in Swagger to create a new user (e.g., email: test@user.com, password: password123).
2. **Log In:** Use the /Auth/login endpoint with the newly created credentials.
3. **Copy JWT:** The successful response will return a Bearer JWT. Copy this token.

### **Step 4: Access Secured Reports**

1. In the Swagger UI, click the green **Authorize** button (top right).
2. In the value field, paste the copied JWT, prefixed by Bearer (e.g., Bearer eyJhbGc...). Click **Authorize** and then **Close**.
3. You can now access the secured reporting endpoints:
   - GET /reports/overview: Get totals across all pages and dates.
   - GET /reports/pages: Get aggregated statistics grouped by page.

## **🗄️ Database Schema Summary**

The primary reporting endpoints pull from the DailyStats table, which holds the aggregated data.

| Table Name | Purpose                                  | Key Fields                                      |
| :--------- | :--------------------------------------- | :---------------------------------------------- |
| Users      | Authentication and Authorization         | Id, Email, PasswordHash                         |
| RawData    | Stores every combined record from GA/PSI | Date, Page, Users, PerformanceScore             |
| DailyStats | **Aggregated Report Data**               | Date, TotalUsers, AvgPerformance, LastUpdatedAt |

## **✨ Bonus Features Implemented**

- **Docker Healthchecks:** Healthchecks are configured in docker-compose.yml for the RabbitMQ and SQL Server containers.
- **Clear Logging:** Detailed console output is provided across the Producer and Consumer services, clearly indicating:
  - Messages published to RabbitMQ.
  - Messages consumed.
  - Successful data save to SQL Server.
  - Detailed retry attempts on transient database failures.

**Happy coding\!**
