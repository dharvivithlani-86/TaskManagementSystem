# Team Task Management System

> **Role-Based Full-Stack Task Management Application Assessment**  
> Built with **.NET 10 Web API**, **Entity Framework Core 10**, **Microsoft SQL Server**, and **React 19 + TypeScript + Tailwind CSS**.

---

## 🌟 Executive Summary

TaskMaster is an enterprise-grade, role-based task and team management platform designed to facilitate cross-functional team coordination, granular task tracking, collaborative discussion, and automated assignment/status notifications.

### Key Highlights
- **Backend**: .NET 10 (`net10.0`) Web API with C# 13, Entity Framework Core 10, and SQL Server.
- **Frontend**: Senior-architected React 19 SPA built with Vite, TypeScript, and Tailwind CSS.
- **Database & Auditing**: Universal `BaseEntity` providing standardized primary keys (`Id`), creation audit (`CreatedAt`, `CreatedBy`), modification audit (`UpdatedAt`, `UpdatedBy`), and soft deletion (`IsDeleted`).
- **Clean & Maintainable Structure**: Single cohesive Web API with cleanly partitioned folders (`Entities`, `Data`, `DTOs`, `Services`, `Controllers`, `Common`, `Middleware`).
- **Centralized Messaging**: **All** error strings, validation rules, status messages, and notification templates reside in a single `AppMessages.cs` file.
- **Global Exception Handling & Logging**: RFC 7807 problem details error format and Serilog structured logging to both console and daily rolling file sinks (`Logs/tms-.log`).
- **Security**: JWT Bearer token authentication with Role-based authorization policies (Admin, Manager, User), BCrypt password hashing, and client-side session expiration interceptors.
- **Notifications**: Dual in-app notification center and mock email dispatch logging triggered upon task assignment and status updates.
- **API Documentation & Tooling**: Swagger UI with interactive JWT authorization and an exported Postman collection with environments.
- **Quality Assurance**: Automated xUnit unit test suite with FluentAssertions and InMemory EF Core.

---

## 👥 Role-Based Capabilities Matrix

| Feature / Action | Admin | Manager | User (Team Member) |
| :--- | :---: | :---: | :---: |
| **View Dashboard Metrics** | All Org Data | Managed Teams & Tasks | Assigned Tasks Only |
| **Create Teams** | ✅ | ✅ | ❌ |
| **Manage Team Members** | ✅ | ✅ (Led Teams) | ❌ |
| **Create Tasks** | ✅ | ✅ | ❌ |
| **Assign Tasks** | Any User | Team Members | ❌ |
| **Update Task Status** | ✅ | ✅ | ✅ (Assigned Tasks) |
| **Delete Tasks** | ✅ | ✅ (Created by self) | ❌ |
| **Comment on Tasks** | ✅ | ✅ | ✅ |
| **Receive Assignment Notifications** | ✅ | ✅ | ✅ |
| **Receive Status Change Notifications** | ✅ | ✅ | ✅ |

---

## 🔑 Pre-Seeded Evaluation Accounts

The database is seeded automatically with three default accounts:

| Role | Email | Password | Pre-Assigned Responsibilities |
| :--- | :--- | :--- | :--- |
| **Admin** | `admin@tms.com` | `Admin@12345` | Global oversight, manages all teams & users |
| **Manager** | `manager@tms.com` | `Manager@12345` | Leads "Core Engineering Team", creates & assigns tasks |
| **User** | `user@tms.com` | `User@12345` | Member of "Core Engineering Team", works on assigned tasks |

> *Note: The login page includes 1-click quick fill buttons for each of these test users.*

---

## 🏗️ Architecture & Project Layout

```
TaskManagement System/
├── TaskManagement.sln               # Visual Studio / .NET solution
├── docker-compose.yml               # Multi-container Docker configuration
├── README.md                        # Documentation and setup guide
│
├── TaskManagement.Server/           # .NET 10 Web API
│   ├── Common/
│   │   ├── AppMessages.cs           # ⭐ Single source of truth for all messages
│   │   ├── ApiResponse.cs           # Uniform response wrapper { success, message, data, errors }
│   │   ├── PagedResult.cs           # Pagination wrapper
│   │   └── Enums.cs                 # UserRole, TaskItemStatus, TaskPriority, NotificationType
│   ├── Data/
│   │   ├── ApplicationDbContext.cs  # EF Core context with automatic audit interceptor
│   │   ├── DbInitializer.cs         # Database seeder (Users, Teams, Tasks, Comments)
│   │   └── Migrations/              # EF Core database migrations
│   ├── Entities/
│   │   ├── BaseEntity.cs            # Common audit properties (Id, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, IsDeleted)
│   │   ├── User.cs
│   │   ├── Team.cs
│   │   ├── TeamMember.cs
│   │   ├── TaskItem.cs
│   │   ├── Comment.cs
│   │   └── NotificationLog.cs
│   ├── DTOs/                        # Request/Response records
│   ├── Services/
│   │   ├── ICurrentUserService.cs   # Extracts user claims from HttpContext
│   │   ├── ITokenService.cs         # JWT generation
│   │   └── INotificationService.cs  # In-app logging & mock email dispatcher
│   ├── Middleware/
│   │   └── GlobalExceptionMiddleware.cs # RFC 7807 ProblemDetails error handling
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── TasksController.cs
│   │   ├── TeamsController.cs
│   │   ├── CommentsController.cs
│   │   ├── NotificationsController.cs
│   │   ├── DashboardController.cs
│   │   └── UsersController.cs
│   ├── appsettings.json
│   └── Program.cs
│
├── TaskManagement.Client/           # React 19 Frontend
│   ├── src/
│   │   ├── api/                     # Axios client with JWT interceptor and API services
│   │   ├── components/
│   │   │   ├── common/              # Navbar, Sidebar, Modal, Badge
│   │   │   ├── tasks/               # KanbanBoard, TaskTable, TaskFilterBar, TaskDetailModal, CreateTaskModal
│   │   │   └── teams/               # TeamCard, CreateTeamModal, ManageMembersModal
│   │   ├── context/                 # AuthContext, NotificationContext
│   │   ├── pages/                   # LoginPage, RegisterPage, DashboardPage, TasksPage, TeamsPage, NotFoundPage
│   │   └── types/                   # TypeScript interfaces matching backend DTOs
│   ├── vite.config.ts
│   └── package.json
│
├── TaskManagement.Tests/            # xUnit Test Suite
│   ├── AuthTests.cs                 # BCrypt & JWT token tests
│   ├── AuditEntityTests.cs          # BaseEntity & soft delete tests
│   └── TaskWorkflowTests.cs         # Task lifecycle & notification triggers
│
├── postman/
│   ├── TaskManagement.postman_collection.json
│   └── TaskManagement.postman_environment.json
│
└── docker/
    ├── Dockerfile.server
    └── Dockerfile.client
```

---

## 🚀 Getting Started (Local Development)

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (or .NET 9/8)
- [Node.js](https://nodejs.org/) (v18+) and npm
- Microsoft SQL Server (LocalDB or MSSQLSERVER instance running locally)

---

### Step 1: Database Setup & Migrations
The connection string in `TaskManagement.Server/appsettings.json` is set to:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;"
}
```
Run the migrations and apply them to your database:
```powershell
dotnet ef database update --project TaskManagement.Server/TaskManagement.Server.csproj
```
*(On application startup, `DbInitializer.cs` will also automatically verify the schema and populate the initial seed data!)*

---

### Step 2: Run the Backend (.NET 10 Web API)
```powershell
cd "TaskManagement.Server"
dotnet run --launch-profile http
```
- API Base URL: `http://localhost:5128`
- Interactive Swagger UI: `http://localhost:5128/swagger`

---

### Step 3: Run the Frontend (React 19)
Open a separate terminal window:
```powershell
cd "TaskManagement.Client"
npm install
npm run dev
```
- Frontend Application URL: `http://localhost:5173`

---

## 🐳 Docker Deployment (Multi-Container)

To spin up the entire application stack including SQL Server, .NET 10 API, and React Frontend:
```powershell
docker-compose up --build -d
```
- Frontend: `http://localhost:5173`
- Backend API: `http://localhost:5128`
- SQL Server: `localhost:1433`

---

## 🧪 Running Automated Tests

Run the full xUnit test suite from the repository root:
```powershell
dotnet test
```

Test coverage includes:
- Verification of BCrypt password hashing & matching.
- JWT security token generation and claim mapping.
- Automatic audit property tracking (`CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`).
- Soft delete behavior and query filter validation.
- Notification dispatch upon task assignment and status updates.

---

## 📮 Postman Collection

1. Open Postman.
2. Click **Import** and select:
   - `postman/TaskManagement.postman_collection.json`
   - `postman/TaskManagement.postman_environment.json`
3. Execute **Login as Admin** or **Login as Manager**. The collection contains automated test scripts that automatically save the issued JWT token into the `token` environment variable for subsequent requests!

---
