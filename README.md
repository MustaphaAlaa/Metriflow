# **Web Analytics Data Aggregator (ElectroPi Quest)**

This repository contains the solution for the ElectroPi "Web Analytics Data Aggregator" quest. The system is designed to ingest raw data from mocked external web analytics sources (Google Analytics and PageSpeed Insights), process it through a real message broker (RabbitMQ), aggregate daily statistics, and expose secure reporting APIs.

## 🔑 Key Features

- **Data Ingestion (JSON Sources)**  
  Reads mock data from **Google Analytics (GA)** and **PageSpeed Insights (PSI)** JSON files. Each record represents a page’s metrics for a specific date. The system standardizes and merges both datasets into unified analytical records.

- **Message Production (RabbitMQ)**  
  A **.NET console producer** simulates real-time API data by publishing GA , and PSI records, one by one, to a RabbitMQ **exchange**, ensuring asynchronous and decoupled processing.

- **Data Correlation & Caching (Redis)**  
  A **.NET Worker Service (Consumer 1)** consumes records from RabbitMQ and temporarily stores them in **Redis** to wait for matching data (by _page_ and _date_). Once a day’s data is complete, it merges the pairs into a single consolidated record and republishes it for aggregation.

- **Aggregation & Persistence (EF Core + SQL Server)**  
  A second **.NET Worker Service (Consumer 2)** processes these consolidated records, computes per-page and per-day totals and averages, and persists the results in **SQL Server** using **Entity Framework Core**.

- **Secure Reporting API (ASP.NET Core)**  
  Provides authenticated endpoints for authorized users to query aggregated analytics (daily, per page, and overview reports) using **JWT-based authentication**.

- **Reliability & Fault Tolerance**  
  Ensures message delivery integrity with **acknowledgment on success**, **retry logic (3 attempts with exponential backoff)**, and **dead-letter queue handling** for failed messages.

- **Containerized Infrastructure**  
  Fully orchestrated with **Docker Compose**, including the **API**, **Worker Services**, **SQL Server**, **RabbitMQ**, and **Redis** — ensuring easy local development and consistent deployment.

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

The system follows a clear, event-driven flow designed for decoupled, reliable, and time-aware data processing.

1. **Data Producer (Console App)**  
   Reads mock **Google Analytics (GA)** and **PageSpeed Insights (PSI)** JSON files and publishes each raw record (as-is) to the **analytics.raw** exchange on **RabbitMQ** to simulate real-time data streaming.

2. **Correlation Worker (Consumer 1)**  
   Subscribed to the **analytics.raw.q** queue.  
   Receives individual GA and PSI records, temporarily caches them in **Redis** until matching records (by _page_ and _date_) are available.  
   At the end of each day, it combines the matching GA + PSI data into a single correlated record and republishes it to the **analytics.daily** exchange.

3. **Aggregation Worker (Consumer 2)**  
   Listens to the **analytics.daily.q** queue, consumes correlated records, calculates per-page and per-day aggregates (totals and averages), and persists the results to **SQL Server** via **EF Core**.

4. **Reporting API (ASP.NET Core)**  
   Exposes the aggregated analytics through **JWT-protected endpoints**, providing reports by day, by page, and overall summaries.

5. **Containerized Environment (Docker Compose)**  
   All components — **API**, **Workers**, **RabbitMQ**, **Redis**, and **SQL Server** — run as isolated containers, ensuring reliable orchestration and consistent local development.

## **⚙️ Setup and Running the Application**

### **Prerequisites**

You must have the following installed:

- [Docker](https://www.docker.com/get-started/)
- Docker Compose (Usually included with Docker Desktop)

### **Step 1: Start All Services**

Navigate to the root directory of the repository where the docker-compose.yml file is located and run:

`docker compose up \--build \-d
`

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
| Page | Store page's path to prevent redundancy data | Id, Path |
| RawData    | Stores every combined record from GA/PSI | Date, PageId, Users, PerformanceScore             |
| DailyStats | **Aggregated Report Data**               | Date, TotalUsers, AvgPerformance, LastUpdatedAt |

## **✨ Bonus Features Implemented**

- **Docker Healthchecks:** Healthchecks are configured in docker-compose.yml for the RabbitMQ and SQL Server containers.
- **Clear Logging:** Detailed console output is provided across the Producer and Consumer services, clearly indicating:
  - Messages published to RabbitMQ.
  - Messages consumed.
  - Successful data save to SQL Server.
  - Detailed retry attempts on transient database failures.

**Happy coding\!**
