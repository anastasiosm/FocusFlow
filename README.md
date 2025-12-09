# FocusFlow - Task Management System

A clean architecture task management application built with .NET 8, demonstrating SOLID principles, CQRS pattern, test-driven development, and modern containerization practices.
 
## 🏗️ Architecture

FocusFlow follows **Onion Architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │
│    (Blazor Server + Web API)           │
├─────────────────────────────────────────┤
│         Application Layer               │
│  (Commands, Queries, DTOs, Validators) │ ✅ COMPLETE
├─────────────────────────────────────────┤
│          Domain Layer                   │
│   (Entities, Business Logic)           │ ✅ COMPLETE
├─────────────────────────────────────────┤
│       Infrastructure Layer              │
│  (EF Core, Identity, Repositories)     │
└─────────────────────────────────────────┘
```

### Key Architectural Decisions

**No BaseEntity Inheritance**
- Each entity (Project, ProjectTask) manages its own properties explicitly
- Simpler code, easier to understand
- Follows YAGNI principle - only 2 entities don't justify abstraction

**FocusFlow-Branded Exceptions**
- `FocusFlowException` (base)
- `FocusFlowValidationException` - Invalid input
- `FocusFlowBusinessRuleException` - Business logic violations
- `FocusFlowNotFoundException` - Entity not found
- `FocusFlowUnauthorizedException` - Unauthorized access

**CQRS with MediatR**
- Commands for writes (CreateProject, UpdateTask)
- Queries for reads (GetAllProjects, GetTasksByProject)
- Thin controllers, fat handlers

## 🚀 Features

- ✅ User authentication & authorization
- ✅ Project management (CRUD)
- ✅ Task management with priorities & statuses
- ✅ Progress dashboards & statistics
- ✅ Real-time updates (SignalR)
- ✅ RESTful API with OpenAPI documentation
- ✅ Containerized deployment (Docker & Kubernetes)

## 🛠️ Tech Stack

### Backend
- **.NET 8** - Latest LTS framework
- **MediatR 12.4** - CQRS pattern implementation
- **FluentValidation 11.10** - Declarative validation
- **AutoMapper 13.0** - Object-to-object mapping
- **Entity Framework Core** - ORM with Code-First migrations
- **ASP.NET Core Identity** - Authentication & authorization
- **Serilog** - Structured logging

### Frontend
- **Blazor Server** - Interactive web UI
- **MudBlazor** - Material Design components
- **SignalR** - Real-time communication

### Testing
- **xUnit** - Test framework
- **FluentAssertions** - Fluent assertion library
- **Moq** - Mocking framework
- **Bogus** - Fake data generation

### DevOps
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration
- **Kubernetes** - Container orchestration (optional)
- **GitHub Actions** - CI/CD pipeline

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)
- IDE: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Rider](https://www.jetbrains.com/rider/) or [VS Code](https://code.visualstudio.com/)

## 🏃 Quick Start

### Local Development

```bash
# Clone repository
git clone https://github.com/YOUR_USERNAME/FocusFlow.git
cd FocusFlow

# Restore dependencies
dotnet restore

# Run tests
dotnet test

# Run application (coming soon)
# dotnet run --project src/FocusFlow.WebApi
```

### Docker Compose (coming soon)

```bash
# Build and run all services
docker compose up -d

# Application will be available at:
# - Web UI: http://localhost:5000
# - API: http://localhost:5001
# - Swagger: http://localhost:5001/swagger

# Stop services
docker compose down
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true

# Run specific test project
dotnet test tests/FocusFlow.Domain.Tests
dotnet test tests/FocusFlow.Application.Tests

# Current coverage:
# - Domain Layer: 50 tests, ~95% coverage ✅
# - Application Layer: Tests coming soon
```

## 📁 Project Structure

```
FocusFlow/
├── src/
│   ├── FocusFlow.Domain/              # ✅ Core entities & business logic
│   │   ├── Entities/
│   │   │   ├── Project.cs
│   │   │   └── ProjectTask.cs
│   │   ├── Enums/
│   │   │   ├── TaskStatus.cs
│   │   │   └── Priority.cs
│   │   └── Exceptions/
│   │       └── FocusFlowException.cs
│   ├── FocusFlow.Application/         # ✅ Use cases, DTOs, validators
│   │   ├── DTOs/
│   │   │   ├── ProjectDto.cs
│   │   │   ├── TaskDto.cs
│   │   │   └── DashboardStatsDto.cs
│   │   ├── Projects/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateProjectCommand.cs
│   │   │   │   ├── UpdateProjectCommand.cs
│   │   │   │   └── DeleteProjectCommand.cs
│   │   │   └── Queries/
│   │   │       ├── GetAllProjectsQuery.cs
│   │   │       └── GetProjectByIdQuery.cs
│   │   ├── Tasks/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   ├── Interfaces/
│   │   │   ├── IProjectRepository.cs
│   │   │   ├── ITaskRepository.cs
│   │   │   └── IUnitOfWork.cs
│   │   ├── Validators/
│   │   │   └── CommandValidators.cs
│   │   └── Mappings/
│   │       └── MappingProfiles.cs
│   ├── FocusFlow.Infrastructure/      # 🚧 EF Core, Identity, Repos
│   ├── FocusFlow.WebApi/              # 🚧 REST API controllers
│   ├── FocusFlow.BlazorApp/           # 🚧 Blazor UI
│   └── FocusFlow.Shared/              # 🚧 Common constants
├── tests/
│   ├── FocusFlow.Domain.Tests/        # ✅ 50 unit tests
│   ├── FocusFlow.Application.Tests/   # 🚧 Coming soon
│   ├── FocusFlow.Infrastructure.Tests/# 🚧 Coming soon
│   └── FocusFlow.Integration.Tests/   # 🚧 Coming soon
├── docker/                            # 🚧 Docker configuration
├── k8s/                               # 🚧 Kubernetes manifests
├── docs/                              # 🚧 Documentation
└── README.md
```

**Legend:** ✅ Complete | 🚧 In Progress | ⏳ Planned

## 🎯 Development Roadmap

### Phase 1: Foundation ✅ COMPLETE
- [x] Solution structure
- [x] Domain entities (Project, ProjectTask)
- [x] Domain exceptions (FocusFlow-branded)
- [x] 50 unit tests with >95% coverage

### Phase 2: Application Layer ✅ COMPLETE
- [x] DTOs (Project, Task, Dashboard)
- [x] CQRS Commands & Queries
- [x] Repository interfaces
- [x] FluentValidation validators
- [x] AutoMapper profiles
- [x] MediatR handlers

### Phase 3: Infrastructure Layer 🚧 IN PROGRESS
- [ ] EF Core DbContext
- [ ] Repository implementations
- [ ] ASP.NET Core Identity setup
- [ ] Database migrations
- [ ] Unit of Work implementation

### Phase 4: Web API 🚧 NEXT
- [ ] API controllers
- [ ] Swagger/OpenAPI configuration
- [ ] Authentication middleware
- [ ] Error handling middleware
- [ ] Integration tests

### Phase 5: Frontend ⏳
- [ ] Blazor project setup
- [ ] Authentication pages
- [ ] Project management UI
- [ ] Task management UI
- [ ] Dashboard with statistics

### Phase 6: Advanced Features ⏳
- [ ] SignalR real-time updates
- [ ] Task filtering & search
- [ ] Overdue task detection
- [ ] Serilog logging

### Phase 7: Containerization ⏳
- [ ] Dockerfiles
- [ ] docker-compose.yml
- [ ] Kubernetes manifests
- [ ] CI/CD pipeline

## 📚 Key Design Decisions (ADR)

### ADR-001: Onion Architecture
**Decision:** Use Onion Architecture instead of traditional N-Tier  
**Rationale:** Better testability, domain-centric design, framework independence  
**Consequences:** More projects but cleaner separation of concerns

### ADR-002: CQRS with MediatR
**Decision:** Implement Command/Query separation using MediatR  
**Rationale:** Clearer use cases, easier testing, single responsibility  
**Consequences:** More classes but more maintainable and testable

### ADR-003: No BaseEntity
**Decision:** No abstract BaseEntity class  
**Rationale:** Only 2 entities, explicit is better than implicit, YAGNI principle  
**Consequences:** Slight duplication but simpler, more understandable code

### ADR-004: FocusFlow Exception Naming
**Decision:** Prefix all domain exceptions with "FocusFlow"  
**Rationale:** Clear branding, avoid conflicts, explicit naming  
**Consequences:** Longer names but better clarity and searchability

### ADR-005: FluentValidation over Data Annotations
**Decision:** Use FluentValidation for all validation logic  
**Rationale:** More expressive, testable, reusable, supports complex rules  
**Consequences:** Extra package but significantly better validation capabilities

### ADR-006: AutoMapper for DTOs
**Decision:** Use AutoMapper for entity-to-DTO mapping  
**Rationale:** DRY principle, convention-based, testable configurations  
**Consequences:** Extra abstraction but eliminates repetitive mapping code

