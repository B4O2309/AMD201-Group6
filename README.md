# 🚀 .NET URL Shortener System (Microservices & CI/CD)

This project is a scalable, performant URL-shortening service built with a **.NET Core Microservices** architecture. It features a professional **CI/CD pipeline**, **Distributed Caching**, and **Asynchronous Communication** between services to meet the highest assessment criteria.

## 👥 Team Members & Responsibilities

Our team consists of 3 software engineers following modern DevOps principles to ensure a fully automated and reliable cloud-native deployment.

### 1. Nguyễn Hoàng Gia Bảo — Infrastructure & URL Service
*Focus: System Architecture, Performance Optimization, and API Routing.*

* **Infrastructure & Gateway:**
    * Configure `docker-compose.yml` to orchestrate all services including SQL Server, Redis, and RabbitMQ.
    * Setup `.gitignore` for .NET and Node.js environments.
    * Implement **API Gateway** (Ocelot/YARP) as the single entry point for the system.
    * Develop **Multi-stage Dockerfile** for the Gateway to optimize production image size.
* **URL Service Development:**
    * Design `URLService/Entities/Url.cs` for data persistence.
    * Implement **Redis Caching** in `CacheService.cs` to improve read performance for shortened URLs.
    * Develop `UrlConsumer.cs` (RabbitMQ) to handle asynchronous events from User Service.
    * Implement core shortening logic to generate unique codes and handle **302 Redirect** functionality.
* **Testing & CI/CD:**
    * Write **Unit Tests** for algorithms and **Integration Tests** for Redis/Database connectivity.
    * Setup `.github/workflows/url-service.yml` for automated build, test, and push to Docker Hub.

### 2. Anh Toàn — User Service & Security
*Focus: User Authentication and Asynchronous Message Brokering.*

* **Documentation:**
    * Maintain `docs/contracts.md` for JSON API standards and RabbitMQ event schemas.
* **User Service Development:**
    * Design `UserService/Entities/User.cs` schema.
    * Implement **JWT Authentication** and password hashing in `AuthService.cs`.
    * Develop **RabbitMQ Publisher** to broadcast user events to the URL shortening service.
    * Expose RESTful APIs for User Registration and Login.
* **Testing & CI/CD:**
    * Write **Unit Tests** for Auth logic and **Integration Tests** for SQL Server.
    * Implement **Multi-stage Dockerfile** for optimized production images.
    * Setup `.github/workflows/user-service.yml` for independent service deployment.

### 3. Hải Quân — Frontend & Cloud DevOps
*Focus: User Interface and Automated Deployment Strategy.*

* **Web UI (React/Vite):**
    * Develop a modern frontend using **React** to interact with the shortening service.
    * Implement `AuthContext.js` for global authentication state management.
    * Setup `api.js` (Axios) for centralized communication with the API Gateway.
    * Build UI components: `ShortenForm`, `UrlTable`, and `Navbar`.
    * Create `Dockerfile` using **Nginx** for serving static assets