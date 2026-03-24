# .NET URL Shortener System (Microservices)

This project is a scalable, performant URL-shortening service built with a **.NET Core Microservices** architecture. It features **Distributed Caching** and **Asynchronous Communication** between services to ensure high availability and performance.

## Team Members & Responsibilities

Our team consists of 3 software engineers focusing on building a robust, cloud-native distributed system using Docker.

### 1. Nguyễn Hoàng Gia Bảo — Infrastructure & URL Service
*Focus: System Architecture, Performance Optimization, and API Routing.*

* **Infrastructure & Gateway:**
    * Configure `docker-compose.yml` to orchestrate all services including SQL Server, Redis, and RabbitMQ.
    * Setup `.gitignore` for .NET and Node.js environments.
    * Implement **API Gateway** (Ocelot) as the single entry point for the system.
    * Develop **Multi-stage Dockerfile** for the Gateway to optimize production image size.
* **URL Service Development:**
    * Design `URLService/Entities/Url.cs` for data persistence.
    * Implement **Redis Caching** in `URLController.cs` to improve read performance for shortened URLs.
    * Develop `UrlConsumer.cs` (RabbitMQ) to handle asynchronous events from User Service.
    * Implement core shortening logic to generate unique codes using Base62 and handle **302 Redirect** functionality.
* **Testing:**
    * Write **Unit Tests** and **Integration Tests** for Redis/Database connectivity.

### 2. Trương Anh Toàn — User Service & Security
*Focus: User Authentication and Asynchronous Message Brokering.*

* **Documentation:**
    * Maintain `docs/contracts.md` for JSON API standards and RabbitMQ event schemas.
* **User Service Development:**
    * Design `UserService/Entities/User.cs` schema.
    * Implement **JWT Authentication** and password hashing in `AuthService.cs`.
    * Develop **RabbitMQ Publisher** to broadcast user events to the URL shortening service.
    * Expose RESTful APIs for User Registration and Login.
* **Testing & Docker:**
    * Write **Unit Tests** for Auth logic and **Integration Tests** for SQL Server.
    * Implement **Multi-stage Dockerfile** for optimized production images.

### 3. Trần Hải Quân — Frontend & Integration
*Focus: User Interface and Service Integration.*

* **Web UI (React/Vite):**
    * Develop a modern frontend using **React** to interact with the shortening service.
    * Implement `AuthContext.js` for global authentication state management.
    * Setup `api.js` (Axios) for centralized communication with the API Gateway.
    * Build UI components: `ShortenForm`, `UrlTable`, and `Navbar`.
* **Deployment Readiness:**
    * Create **Dockerfile** for the React application to run in a containerized environment.
    * Ensure the frontend correctly routes all requests through the Ocelot API Gateway.

## 🛠 Tech Stack

* **Backend:** .NET 8, Entity Framework Core
* **Frontend:** React (Vite), Tailwind CSS
* **Database:** SQL Server
* **Caching:** Redis
* **Messaging:** RabbitMQ
* **Gateway:** Ocelot
* **Containerization:** Docker, Docker-compose