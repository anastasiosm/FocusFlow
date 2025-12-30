# SignalR + Fluxor Real-time Architecture

## Overview

This document describes the implementation of real-time task updates using SignalR and Fluxor in the FocusFlow application. The architecture enables live updates across multiple browser sessions when tasks are created, updated, or deleted.

## High-Level Architecture

### Project Structure Overview

```
📁 FocusFlow.Application/
├── Common/
│   └── Events/
│       └── IEventPublisher.cs              ✅ Interface (abstraction)
└── Features/
    └── Tasks/
        ├── CreateTask/
        │   └── CreateTaskCommandHandler.cs ✅ Uses IEventPublisher
        ├── UpdateTask/
        │   └── UpdateTaskCommandHandler.cs ✅ Uses IEventPublisher
        ├── UpdateTaskStatus/
        │   └── UpdateTaskStatusCommandHandler.cs ✅ Uses IEventPublisher
        └── DeleteTask/
            └── DeleteTaskCommandHandler.cs ✅ Uses IEventPublisher

📁 FocusFlow.Infrastructure/
├── SignalR/
│   └── SignalREventPublisher.cs            ✅ Implementation of IEventPublisher
└── DependencyInjection.cs                 ✅ Service registration

📁 FocusFlow.WebApi/
├── Hubs/
│   └── TasksHub.cs                         ✅ SignalR Hub endpoint
└── Program.cs                              ✅ Hub registration & CORS

📁 FocusFlow.BlazorApp/
├── Shared/
│   ├── Models/SignalR/
│   │   └── TaskNotifications.cs            ✅ Shared DTOs
│   ├── Services/SignalR/
│   │   ├── ISignalRService.cs              ✅ Client service interface
│   │   ├── SignalRService.cs               ✅ SignalR client implementation
│   │   └── SignalRTasksListener.cs         ✅ SignalR → Fluxor bridge
│   └── Components/
│       └── SignalRConnectionManager.razor  ✅ Connection lifecycle
├── Features/
│   ├── Tasks/List/Store/
│   │   ├── TasksListActions.cs             ✅ SignalR actions
│   │   ├── TasksListEffects.cs             ✅ SignalR effects
│   │   └── TasksListReducers.cs            ✅ SignalR reducers
│   └── Projects/Detail/Store/
│       ├── ProjectDetailActions.cs         ✅ SignalR actions
│       ├── ProjectDetailEffects.cs         ✅ SignalR effects
│       └── ProjectDetailReducers.cs        ✅ SignalR reducers
├── Services/
│   ├── ITokenProvider.cs                   ✅ Authentication interface
│   └── TokenProvider.cs                    ✅ JWT token management
├── Auth/
│   └── BlazorAuthenticationHandler.cs      ✅ SignalR authentication
├── Components/
│   └── App.razor                           ✅ SignalR initialization
└── Program.cs                              ✅ Service registration
```

## Runtime Flow Diagram

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Browser Tab 1 │    │   Browser Tab 2 │    │   Browser Tab N │
│                 │    │                 │    │                 │
│  ┌─────────────┐│    │  ┌─────────────┐│    │  ┌─────────────┐│
│  │ Fluxor Store││    │  │ Fluxor Store││    │  │ Fluxor Store││
│  └─────────────┘│    │  └─────────────┘│    │  └─────────────┘│
│         ▲       │    │         ▲       │    │         ▲       │
│         │       │    │         │       │    │         │       │
│  ┌─────────────┐│    │  ┌─────────────┐│    │  ┌─────────────┐│
│  │SignalRTasks ││    │  │SignalRTasks ││    │  │SignalRTasks ││
│  │  Listener   ││    │  │  Listener   ││    │  │  Listener   ││
│  └─────────────┘│    │  └─────────────┘│    │  └─────────────┘│
│         ▲       │    │         ▲       │    │         ▲       │
│         │       │    │         │       │    │         │       │
│  ┌─────────────┐│    │  ┌─────────────┐│    │  ┌─────────────┐│
│  │SignalR Client│    │  │SignalR Client│    │  │SignalR Client│
│  └─────────────┘│    │  └─────────────┘│    │  └─────────────┘│
└─────────▲───────┘    └─────────▲───────┘    └─────────▲───────┘
          │                      │                      │
          └──────────────────────┼──────────────────────┘
                                 │
                    ┌─────────────┴─────────────┐
                    │      SignalR Hub          │
                    │     (TasksHub)            │
                    └─────────────▲─────────────┘
                                 │
                    ┌─────────────┴─────────────┐
                    │   SignalREventPublisher   │
                    │   (IEventPublisher)       │
                    └─────────────▲─────────────┘
                                 │
                    ┌─────────────┴─────────────┐
                    │   Command Handlers        │
                    │   (Application Layer)     │
                    └───────────────────────────┘
```

## Components Overview

### Backend Components

#### 1. IEventPublisher Interface
**Location**: `src/FocusFlow.Application/Common/Events/IEventPublisher.cs`

```csharp
public interface IEventPublisher
{
    Task PublishTaskCreatedAsync(Guid taskId, Guid projectId);
    Task PublishTaskUpdatedAsync(Guid taskId, Guid projectId);
    Task PublishTaskStatusChangedAsync(Guid taskId, Guid projectId, ProjectTaskStatus newStatus);
    Task PublishTaskDeletedAsync(Guid taskId, Guid projectId);
}
```

**Purpose**: Abstraction for publishing task-related events. Allows the Application layer to publish events without knowing about SignalR implementation details.

#### 2. SignalREventPublisher
**Location**: `src/FocusFlow.Infrastructure/SignalR/SignalREventPublisher.cs`

**Purpose**: Concrete implementation of `IEventPublisher` that sends SignalR notifications to connected clients.

**Key Features**:
- Uses `IHubContext<TasksHub>` to send messages
- Sends notifications to specific project groups
- Handles connection errors gracefully

#### 3. TasksHub
**Location**: `src/FocusFlow.WebApi/Hubs/TasksHub.cs`

```csharp
[Authorize]
public class TasksHub : Hub
{
    public async Task JoinProjectAsync(string projectId)
    public async Task LeaveProjectAsync(string projectId)
}
```

**Purpose**: SignalR Hub that manages client connections and project-based groups.

**Key Features**:
- Requires authentication (`[Authorize]`)
- Manages project-based groups for targeted notifications
- Clients join/leave project groups based on current page

#### 4. Command Handler Integration
**Modified Files**:
- `CreateTaskCommandHandler.cs`
- `UpdateTaskCommandHandler.cs`
- `UpdateTaskStatusCommandHandler.cs`
- `DeleteTaskCommandHandler.cs`

**Integration Pattern**:
```csharp
public async Task<Result> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
{
    // ... business logic
    
    // Publish SignalR event
    await _eventPublisher.PublishTaskCreatedAsync(task.Id, task.ProjectId);
    
    return Result.Success();
}
```

### Frontend Components

#### 1. SignalR Service Layer

##### ISignalRService Interface
**Location**: `src/FocusFlow.BlazorApp/Shared/Services/SignalR/ISignalRService.cs`

**Purpose**: Abstraction for SignalR client operations.

##### SignalRService Implementation
**Location**: `src/FocusFlow.BlazorApp/Shared/Services/SignalR/SignalRService.cs`

**Key Features**:
- Manages SignalR connection lifecycle
- Handles authentication with JWT tokens
- Provides project group management
- Exposes C# events for task notifications
- Automatic reconnection handling

**Event Definitions**:
```csharp
public event Func<TaskCreatedNotification, Task>? OnTaskCreated;
public event Func<TaskUpdatedNotification, Task>? OnTaskUpdated;
public event Func<TaskStatusChangedNotification, Task>? OnTaskStatusChanged;
public event Func<TaskDeletedNotification, Task>? OnTaskDeleted;
```

#### 2. SignalR-Fluxor Bridge

##### SignalRTasksListener
**Location**: `src/FocusFlow.BlazorApp/Shared/Services/SignalR/SignalRTasksListener.cs`

**Purpose**: Bridge between SignalR events and Fluxor actions. This is the key component that maintains unidirectional data flow.

**Architecture Pattern**:
```csharp
private Task HandleTaskCreated(TaskCreatedNotification notification)
{
    // Convert SignalR notification → Fluxor actions
    _dispatcher.Dispatch(new TasksListActions.TaskCreatedFromSignalRAction(
        notification.TaskId, notification.ProjectId));
    
    _dispatcher.Dispatch(new TaskCreatedInProjectFromSignalRAction(
        notification.TaskId, notification.ProjectId));
    
    return Task.CompletedTask;
}
```

**Why This Pattern?**:
- Keeps Fluxor as single source of truth
- Maintains unidirectional data flow
- SignalR updates go through same flow as user actions
- Enables time-travel debugging (Fluxor DevTools work!)

#### 3. Fluxor State Management

##### TasksList State (for /tasks page)
**Files**:
- `TasksListActions.cs` - SignalR-specific actions
- `TasksListEffects.cs` - Effects that fetch updated data
- `TasksListReducers.cs` - State updates

##### ProjectDetail State (for /projects/{id} page)
**Files**:
- `ProjectDetailActions.cs` - SignalR-specific actions
- `ProjectDetailEffects.cs` - Effects that fetch updated data
- `ProjectDetailReducers.cs` - State updates

**Dual State Update Pattern**:
The `SignalRTasksListener` dispatches actions to **both** states simultaneously, ensuring real-time updates work on both `/tasks` and `/projects/{id}` pages.

#### 4. Authentication Integration

##### TokenProvider Enhancement
**Location**: `src/FocusFlow.BlazorApp/Services/TokenProvider.cs`

**Key Changes**:
- Added async methods for `ProtectedLocalStorage`
- Static cache for token sharing across scoped instances
- Thread-safe initialization with lazy loading

##### BlazorAuthenticationHandler
**Location**: `src/FocusFlow.BlazorApp/Auth/BlazorAuthenticationHandler.cs`

**Purpose**: Ensures SignalR connections use proper JWT authentication.

## Data Flow

### 1. Task Creation Flow

```
User creates task in Tab 1
         ↓
CreateTaskCommand → Handler
         ↓
Task saved to database
         ↓
SignalREventPublisher.PublishTaskCreatedAsync()
         ↓
TasksHub sends notification to project group
         ↓
All connected clients receive notification
         ↓
SignalRTasksListener converts to Fluxor actions
         ↓
Effects fetch updated task data from API
         ↓
Reducers update both TasksList and ProjectDetail states
         ↓
UI re-renders in all tabs showing new task
```

### 2. Real-time Update Flow

```
SignalR Notification
         ↓
SignalRTasksListener.HandleTaskCreated()
         ↓
Dispatch TaskCreatedFromSignalRAction (TasksList)
Dispatch TaskCreatedInProjectFromSignalRAction (ProjectDetail)
         ↓
TasksListEffects.HandleTaskCreatedFromSignalR()
ProjectDetailEffects.HandleTaskCreatedInProjectFromSignalR()
         ↓
API calls to fetch fresh task data
         ↓
Dispatch success actions with task data
         ↓
Reducers update states
         ↓
UI components re-render with new data
```

## Key Design Decisions

### 1. Why Not Domain Events?

**Current Approach**: Events published directly from Command Handlers
```csharp
// In Handler
await _eventPublisher.PublishTaskCreatedAsync(task.Id, task.ProjectId);
```

**Alternative**: Domain Events from Entities
```csharp
// In Entity
public void Complete() 
{
    Status = TaskStatus.Completed;
    AddDomainEvent(new TaskCompletedEvent(Id));
}
```

**Decision Rationale**:
- **Simplicity**: Less infrastructure code required
- **Direct Control**: Explicit control over when events are published
- **Faster Implementation**: Quicker to implement and test
- **Sufficient for Current Needs**: Meets requirements without over-engineering

### 2. Why Dual State Updates?

**Problem**: Users can be on different pages:
- `/tasks` - displays tasks from `TasksListState`
- `/projects/{id}` - displays tasks from `ProjectDetailState.Project.Tasks`

**Solution**: `SignalRTasksListener` dispatches to both states
```csharp
// Update both states simultaneously
_dispatcher.Dispatch(new TasksListActions.TaskCreatedFromSignalRAction(...));
_dispatcher.Dispatch(new TaskCreatedInProjectFromSignalRAction(...));
```

**Benefits**:
- Real-time updates work on all pages
- No duplicate SignalR connections needed
- Consistent user experience

### 3. Why Bridge Pattern (SignalRTasksListener)?

**Alternative**: Direct SignalR → Component updates
```csharp
// BAD: Direct updates
signalRService.OnTaskCreated += (notification) => {
    // Update component state directly
    tasks.Add(newTask);
    StateHasChanged();
};
```

**Current**: SignalR → Fluxor Actions → State Updates
```csharp
// GOOD: Through Fluxor
signalRService.OnTaskCreated += (notification) => {
    _dispatcher.Dispatch(new TaskCreatedFromSignalRAction(...));
};
```

**Benefits**:
- Maintains unidirectional data flow
- Fluxor DevTools work for debugging
- Consistent state management patterns
- Easier testing and reasoning about state changes

## Configuration

### Backend Configuration

#### Program.cs (WebApi)
```csharp
// Add SignalR
builder.Services.AddSignalR();

// Map hub
app.MapHub<TasksHub>("/hubs/tasks");
```

#### DependencyInjection.cs (Infrastructure)
```csharp
services.AddScoped<IEventPublisher, SignalREventPublisher>();
```

### Frontend Configuration

#### Program.cs (BlazorApp)
```csharp
// Register SignalR services
builder.Services.AddScoped<ISignalRService, SignalRService>();
builder.Services.AddScoped<SignalRTasksListener>();
```

#### App.razor
```razor
<!-- Initialize SignalR connection and listener -->
<SignalRConnectionManager />
```

## Testing Strategy

### Backend Testing
- **Unit Tests**: Command handlers with mocked `IEventPublisher`
- **Integration Tests**: End-to-end SignalR message flow
- **Hub Tests**: Connection and group management

### Frontend Testing
- **Unit Tests**: Fluxor actions, effects, and reducers
- **Component Tests**: UI updates with mocked SignalR service
- **E2E Tests**: Multi-tab real-time update scenarios

## Performance Considerations

### Connection Management
- **Project Groups**: Only users viewing a project receive its notifications
- **Automatic Cleanup**: Connections cleaned up when users navigate away
- **Reconnection**: Automatic reconnection on connection loss

### State Updates
- **Efficient Reducers**: Only update necessary parts of state
- **Duplicate Prevention**: Check for existing tasks before adding
- **Lazy Loading**: Fetch full task data only when needed

## Security

### Authentication
- **JWT Tokens**: SignalR connections authenticated with JWT
- **Hub Authorization**: `[Authorize]` attribute on TasksHub
- **Project-based Groups**: Users only receive notifications for projects they have access to

### Data Validation
- **Server-side Validation**: All task operations validated on server
- **Client-side Sync**: Client state synced with server data via API calls

## Monitoring and Debugging

### Logging
- **Structured Logging**: Comprehensive logging throughout the flow
- **SignalR Events**: Log all SignalR events with task and project IDs
- **Connection Status**: Log connection/disconnection events

### Debugging Tools
- **Fluxor DevTools**: Time-travel debugging for state changes
- **SignalR Logging**: Enable detailed SignalR client logging
- **Browser DevTools**: Monitor WebSocket connections

## Future Enhancements

### Potential Improvements
1. **Domain Events**: Refactor to use proper Domain Events pattern
2. **Event Sourcing**: Store all events for audit trail
3. **Offline Support**: Queue events when connection is lost
4. **Optimistic Updates**: Update UI immediately, sync with server later
5. **Selective Updates**: Only send changed fields, not full objects

### Scalability Considerations
1. **Redis Backplane**: For multi-server deployments
2. **Connection Pooling**: Optimize connection management
3. **Message Batching**: Batch multiple updates for efficiency
4. **Caching Strategy**: Cache frequently accessed data

## Troubleshooting

### Common Issues

#### SignalR Connection Fails
- Check JWT token validity
- Verify hub endpoint configuration
- Check CORS settings

#### Events Not Received
- Verify client is in correct project group
- Check SignalR connection status
- Verify event publisher is called in handlers

#### UI Not Updating
- Check Fluxor state updates in DevTools
- Verify component subscribes to correct state
- Check for JavaScript errors in console

#### Authentication Issues
- Verify TokenProvider has valid token
- Check BlazorAuthenticationHandler configuration
- Verify JWT token format and claims

---

## Summary

This SignalR + Fluxor architecture provides a robust, scalable solution for real-time task updates in the FocusFlow application. The bridge pattern maintains clean separation of concerns while enabling seamless real-time collaboration across multiple browser sessions.

The implementation prioritizes simplicity and maintainability while providing a solid foundation for future enhancements.