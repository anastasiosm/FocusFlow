# Architecture Decision Records (ADR)

This document contains the detailed log of key architectural decisions made for FocusFlow, including rationale, consequences, and code examples.

---

### ADR-001: Onion Architecture Over N-Tier
**Decision:** Use Onion Architecture (Clean Architecture variant)  
**Rationale:** 
- Domain layer has **zero dependencies** (pure business logic)
- Easier to test (mock infrastructure)
- Framework-agnostic domain layer
- Enforces dependency inversion (infrastructure depends on domain, not vice versa)

**Consequences:**  
✅ Better testability (~95% domain coverage)  
✅ Business logic isolated from infrastructure changes  
⚠️ More projects (4 layers) but clearer separation

---

### ADR-002: CQRS with MediatR
**Decision:** Separate Commands (writes) from Queries (reads) using MediatR  
**Rationale:**
- **Single Responsibility** - Each handler does one thing
- **Testability** - Mock `IMediator` instead of 10+ service methods
- **Performance** - Queries can bypass domain validation
- **Clarity** - `CreateProjectCommand` vs `GetAllProjectsQuery` is self-documenting

**Consequences:**  
✅ 100+ handlers, each <50 lines  
✅ Easy to add new features (just add handler)  
⚠️ More files, but organized by feature (Vertical Slice)

**Example:**
```csharp
// Command (write)
public record CreateProjectCommand(string Name, DateTime StartDate) : IRequest<Result<ProjectDto>>;

// Query (read)
public record GetAllProjectsQuery(string UserId) : IRequest<Result<List<ProjectDto>>>;
```

---

### ADR-003: Vertical Slice Architecture in Application Layer
**Decision:** Organize by feature (Tasks/, Projects/) instead of technical layer (Commands/, Queries/)  
**Rationale:**
- **Cohesion** - All "Create Task" artifacts in one folder
- **Discoverability** - Easy to find related code
- **Team scalability** - Features can be developed independently

**Structure:**
```
Tasks/
  Commands/
    CreateTaskCommand.cs
    CreateTaskCommandHandler.cs
    CreateTaskCommandValidator.cs
  Queries/
    GetTaskByIdQuery.cs
    GetTaskByIdQueryHandler.cs
  Dtos/
    TaskDto.cs
```

---

### ADR-004: FocusFlow-Branded Exceptions
**Decision:** Prefix all domain exceptions with `FocusFlow` (e.g., `FocusFlowNotFoundException`)  
**Rationale:**
- **Explicit** - No confusion with framework exceptions
- **Searchable** - Easy to find in logs
- **Branding** - Clear ownership of exception types

**Consequences:**  
✅ No namespace conflicts  
✅ Global exception handler easily maps to HTTP status codes  
⚠️ Longer names

**Exception Hierarchy:**
```
FocusFlowException (base)
├── FocusFlowValidationException → 400 Bad Request
├── FocusFlowBusinessRuleException → 422 Unprocessable Entity
├── FocusFlowNotFoundException → 404 Not Found
└── FocusFlowUnauthorizedException → 403 Forbidden
```

---

### ADR-005: FluentValidation Over Data Annotations
**Decision:** Use FluentValidation for all validation logic  
**Rationale:**
- **Testable** - Validators are POCO classes
- **Expressive** - `RuleFor(x => x.EndDate).GreaterThan(x => x.StartDate)`
- **Reusable** - Compose validators
- **Complex rules** - Cross-property, async DB checks

**Consequences:**  
✅ Validation lives in Application layer (not Domain)  
✅ MediatR pipeline runs all validators automatically  
⚠️ Extra package dependency

**Example:**
```csharp
public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required")
            .MaximumLength(200);
        
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .When(x => x.EndDate.HasValue);
    }
}
```

---

### ADR-006: AutoMapper for DTOs
**Decision:** Use AutoMapper for Entity↔DTO mapping  
**Rationale:**
- **DRY** - Define mapping once, use everywhere
- **Convention-based** - Automatically maps properties with same name
- **Testable** - `AssertConfigurationIsValid()`

**Consequences:**  
✅ Eliminates 500+ lines of manual mapping code  
✅ Profiles document transformations  
⚠️ "Magic" behavior (convention-based)

---

### ADR-007: Fluxor for Blazor State Management
**Decision:** Use Fluxor (Redux pattern) instead of Blazor's built-in state containers  
**Rationale:**
- **Predictable** - Single source of truth
- **DevTools** - Redux DevTools support (time-travel debugging)
- **Testable** - Reducers are pure functions
- **Scalable** - Clear action → effect → reducer flow

**Consequences:**  
✅ Easier to debug state changes  
✅ Supports complex async workflows (e.g., optimistic updates)  
⚠️ Learning curve for developers unfamiliar with Redux

**Flow:**
```
Component → Dispatch(CreateProjectAction) 
  → CreateProjectEffect (async API call) 
    → Dispatch(CreateProjectSuccessAction) 
      → ProjectReducer (updates state) 
        → Component re-renders
```

---

### ADR-008: MudBlazor for UI Components
**Decision:** Use MudBlazor over Bootstrap or custom components  
**Rationale:**
- **Rich components** - DataGrid, DatePicker, Autocomplete, Dialogs
- **Material Design** - Modern, consistent UI
- **Accessibility** - ARIA attributes built-in
- **Active maintenance** - 7k+ GitHub stars

**Consequences:**  
✅ Faster UI development  
✅ Responsive grid system  
⚠️ ~2MB bundle size (but tree-shakeable)

---

### ADR-009: Playwright for E2E Tests
**Decision:** Use Playwright over Selenium  
**Rationale:**
- **Auto-wait** - No `Thread.Sleep()` or manual waits
- **Cross-browser** - Chromium, Firefox, WebKit
- **Video/screenshot** - Automatic on test failure
- **Modern API** - Async/await, auto-retry

**Consequences:**  
✅ Reliable E2E tests (no flakiness)  
✅ Debugging with video recordings  
⚠️ Requires browser installation (~300MB)

---

### ADR-010: PostgreSQL Over SQL Server
**Decision:** Use PostgreSQL as the primary database  
**Rationale:**
- **Open-source** - No licensing costs
- **Docker-friendly** - Official image, easy setup
- **JSON support** - Native JSONB type (future analytics)
- **Performance** - Better concurrency with MVCC

**Consequences:**  
✅ Easy local development (Docker)  
✅ Cloud-agnostic (AWS RDS, Azure PostgreSQL, etc.)  
⚠️ Different from SQL Server (no `IDENTITY`, uses `SERIAL`)

---

### ADR-011: JWT Authentication for API
**Decision:** Use JWT tokens for API authentication (not session cookies)  
**Rationale:**
- **Stateless** - No server-side session storage
- **Scalable** - Works across multiple API instances
- **Mobile-friendly** - Easy to use in non-browser clients
- **Claims-based** - Roles embedded in token

**Consequences:**  
✅ Blazor app stores JWT in LocalStorage  
✅ API validates token signature (no DB lookup per request)  
⚠️ Token refresh logic needed (implement refresh tokens)

---

### ADR-012: Docker Compose for Local Development
**Decision:** Provide `docker-compose.yml` as the primary local dev environment  
**Rationale:**
- **Consistency** - Same environment for all developers
- **Database included** - No manual PostgreSQL setup
- **CI/CD parity** - Mirrors production deployment
- **Fast onboarding** - `docker-compose up` = working app

**Consequences:**  
✅ New developers productive in <5 minutes  
✅ Tests run against real database (not in-memory SQLite)  
⚠️ Requires Docker Desktop (~4GB RAM)

---

### ADR-013: HTTP-Only Development Mode
**Decision:** Run Docker containers with HTTP (not HTTPS) in development  
**Rationale:**
- **Simplicity** - No certificate setup for first-time users
- **Faster** - No HTTPS overhead
- **Localhost** - Browsers allow HTTP on localhost

**Consequences:**  
✅ `docker-compose up` works immediately  
✅ No certificate errors  
⚠️ Production must enable HTTPS (see `scripts/setup-dev-certs.ps1` for cert generation)

---

### ADR-014: Repository Pattern with Unit of Work
**Decision:** Use Repository + UnitOfWork pattern despite EF Core's DbContext already being a UoW  
**Rationale:**
- **Testability** - Mock `IProjectRepository` instead of DbContext
- **Abstraction** - Business logic doesn't know about EF Core
- **Complex queries** - Encapsulate in repository methods

**Consequences:**  
✅ Easy to test Application layer (mock repositories)  
✅ Could swap EF Core for Dapper/ADO.NET without changing handlers  
⚠️ Extra abstraction layer (some argue it's redundant)

---

### ADR-015: bUnit for Blazor Component Testing
**Decision:** Use bUnit for testing Blazor components  
**Rationale:**
- **In-memory** - No browser needed
- **Fast** - ~100ms per test
- **Queries** - Find elements like `Find("button.save")`
- **Event simulation** - Click, input, etc.

**Consequences:**  
✅ Tests Blazor components in isolation  
✅ Catches UI bugs before E2E tests  
⚠️ Cannot test CSS/layout (need E2E for that)

---

### ADR-016: Testcontainers for E2E Test Environment
**Decision:** Use Testcontainers to orchestrate real Docker containers (PostgreSQL + API + Blazor) for E2E tests  
**Rationale:**
- **Real environment** - Tests against actual PostgreSQL, not in-memory SQLite
- **Container parity** - Same Docker images used in production
- **Isolation** - Fresh database per test suite
- **Networking** - Tests inter-container communication (API ↔ DB, Blazor ↔ API)
- **DataProtection** - Validates shared key scenarios between API and Blazor

**Consequences:**  
✅ E2E tests catch Docker configuration issues  
✅ No "works on my machine" - consistent test environment  
✅ Tests real database migrations and seeding  
⚠️ Slower than in-memory tests (~2-3 minutes startup)  
⚠️ Requires Docker Desktop with sufficient resources (4GB+ RAM)

**Implementation:**
```csharp
// E2ETestEnvironment orchestrates 3 containers:
// 1. PostgreSQL (fresh DB per test suite)
// 2. API container (focusflow-api:test image)  
// 3. Blazor container (focusflow-client:test image)
// + Custom Docker network for inter-container communication
// + Shared DataProtection keys volume
```

---

### ADR-017: SignalR for Real-Time Updates
**Decision:** Use SignalR for real-time bi-directional communication (Server → Client).
**Rationale:**
- **Abstraction** - Handles WebSockets/Long Polling automatically.
- **Groups** - Easy to broadcast to specific users (e.g., Project Members).
- **Integration** - Native ASP.NET Core support.

**Consequences:**
✅ Instant updates across multiple tabs/users.
✅ Reduced need for manual refreshing.
⚠️ **Integration Complexity:** Must be bridged to Fluxor (via Listeners) to ensure state consistency; creates a second path for data updates alongside REST API.

**Bridge Pattern (SignalR → Fluxor):**
```csharp
// SignalR Listener intercepts the event
hubConnection.On<TaskCreatedNotification>("TaskCreated", notification => 
{
    // Dispatches it as a standard Fluxor Action
    dispatcher.Dispatch(new TaskCreatedAction(notification.Task));
});
```