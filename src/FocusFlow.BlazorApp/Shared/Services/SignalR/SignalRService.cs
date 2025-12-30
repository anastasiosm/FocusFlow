using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Components;
using FocusFlow.BlazorApp.Shared.Models.SignalR;
using FocusFlow.BlazorApp.Services;

namespace FocusFlow.BlazorApp.Shared.Services.SignalR;

/// <summary>
/// SignalR client implementation for Blazor Server.
/// 
/// KEY RESPONSIBILITIES:
/// 1. Establish WebSocket connection to server
/// 2. Handle authentication (JWT token)
/// 3. Handle reconnection automatically
/// 4. Raise C# events when messages are received
/// </summary>
public class SignalRService : ISignalRService, IAsyncDisposable
{
    private readonly ILogger<SignalRService> _logger;
    private readonly ITokenProvider _tokenProvider;
    private readonly IConfiguration _configuration;
    private HubConnection? _hubConnection;

    // C# events that other services can subscribe to
    public event Func<TaskCreatedNotification, Task>? OnTaskCreated;
    public event Func<TaskUpdatedNotification, Task>? OnTaskUpdated;
    public event Func<TaskStatusChangedNotification, Task>? OnTaskStatusChanged;
    public event Func<TaskDeletedNotification, Task>? OnTaskDeleted;

    public bool IsConnected =>
        _hubConnection?.State == HubConnectionState.Connected;

    public SignalRService(
        ILogger<SignalRService> logger,
        ITokenProvider tokenProvider,
        IConfiguration configuration)
    {
        _logger = logger;
        _tokenProvider = tokenProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// Establishes connection to SignalR hub on the server.
    /// Should be called once on app startup (after authentication).
    /// </summary>
    public async Task StartAsync()
    {
        if (_hubConnection != null)
        {
            _logger.LogWarning("⚠️ SignalR connection already exists");
            return;
        }

        try
        {
            // Get token (async for ProtectedLocalStorage)
            var token = await _tokenProvider.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("⚠️ No token available, cannot start SignalR");
                return;
            }

            // Get API URL from configuration
            var apiBaseUrl = _configuration.GetValue<string>("ApiBaseUrl") ?? "http://focusflow-api:8080";
            var hubUrl = $"{apiBaseUrl}/hubs/tasks";

            _logger.LogInformation("🔌 Connecting to SignalR hub: {HubUrl}", hubUrl);

            // Create and configure HubConnection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    // Attach JWT token to connection
                    options.AccessTokenProvider = () => Task.FromResult<string?>(token);

                    // Configure for Blazor Server (not WASM!)
                    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets;
                    
                    // Configure for local development - accept any SSL certificate
                    options.HttpMessageHandlerFactory = (handler) =>
                    {
                        if (handler is HttpClientHandler clientHandler)
                        {
                            clientHandler.ServerCertificateCustomValidationCallback =
                                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                        }
                        return handler;
                    };
                })
                // Automatic reconnection with exponential backoff
                .WithAutomaticReconnect(new[]
                {
                    TimeSpan.Zero,           // Immediate
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(10),
                    TimeSpan.FromSeconds(30)
                })
                .ConfigureLogging(logging =>
                {
                    logging.AddDebug();
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .Build();

            // 4. Register message handlers (see next method)
            RegisterMessageHandlers();

            // 5. Register lifecycle event handlers
            _hubConnection.Reconnecting += OnReconnecting;
            _hubConnection.Reconnected += OnReconnected;
            _hubConnection.Closed += OnClosed;

            // 6. Actually start the connection
            await _hubConnection.StartAsync();

            _logger.LogInformation("✅ SignalR connected successfully | ConnectionId: {ConnectionId}",
                _hubConnection.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to start SignalR connection");
            throw;
        }
    }

    /// <summary>
    /// Registers handlers for incoming messages from server.
    /// This is where we convert SignalR messages to C# events.
    /// 
    /// CRITICAL: The method names ("TaskCreated", "TaskUpdated", etc.) MUST match
    /// what the server sends in SignalREventPublisher!
    /// </summary>
    private void RegisterMessageHandlers()
    {
        if (_hubConnection == null) return;

        // When server sends "TaskCreated", call this handler
        _hubConnection.On<TaskCreatedNotification>("TaskCreated", async (notification) =>
        {
            _logger.LogInformation("📨 Received TaskCreated | TaskId: {TaskId}",
                notification.TaskId);

            // Raise C# event so listeners (like SignalRTasksListener) can react
            if (OnTaskCreated != null)
                await OnTaskCreated.Invoke(notification);
        });

        _hubConnection.On<TaskUpdatedNotification>("TaskUpdated", async (notification) =>
        {
            _logger.LogInformation("📨 Received TaskUpdated | TaskId: {TaskId}",
                notification.TaskId);

            if (OnTaskUpdated != null)
                await OnTaskUpdated.Invoke(notification);
        });

        _hubConnection.On<TaskStatusChangedNotification>("TaskStatusChanged", async (notification) =>
        {
            _logger.LogInformation("📨 Received TaskStatusChanged | TaskId: {TaskId} | Status: {Status}",
                notification.TaskId, notification.NewStatus);

            if (OnTaskStatusChanged != null)
                await OnTaskStatusChanged.Invoke(notification);
        });

        _hubConnection.On<TaskDeletedNotification>("TaskDeleted", async (notification) =>
        {
            _logger.LogInformation("📨 Received TaskDeleted | TaskId: {TaskId}",
                notification.TaskId);

            if (OnTaskDeleted != null)
                await OnTaskDeleted.Invoke(notification);
        });
    }

    /// <summary>
    /// Call this when user navigates to a project page.
    /// Tells server: "Send me updates for THIS project only"
    /// </summary>
    public async Task JoinProjectAsync(Guid projectId)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            _logger.LogWarning("⚠️ Cannot join project - not connected");
            return;
        }

        try
        {
            // Invoke server method to join group
            await _hubConnection.InvokeAsync("JoinProject", projectId.ToString());
            _logger.LogInformation("👥 Joined project group: {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to join project: {ProjectId}", projectId);
        }
    }

    /// <summary>
    /// Call this when user navigates away from a project.
    /// Tells server: "Stop sending me updates for this project"
    /// </summary>
    public async Task LeaveProjectAsync(Guid projectId)
    {
        if (_hubConnection?.State != HubConnectionState.Connected)
        {
            return;
        }

        try
        {
            await _hubConnection.InvokeAsync("LeaveProject", projectId.ToString());
            _logger.LogInformation("👋 Left project group: {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to leave project: {ProjectId}", projectId);
        }
    }

    public async Task StopAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
            _logger.LogInformation("🔌 SignalR connection stopped");
        }
    }

    // Lifecycle event handlers
    private Task OnReconnecting(Exception? exception)
    {
        _logger.LogWarning(exception, "🔄 SignalR reconnecting...");
        return Task.CompletedTask;
    }

    private Task OnReconnected(string? connectionId)
    {
        _logger.LogInformation("✅ SignalR reconnected | ConnectionId: {ConnectionId}", connectionId);
        return Task.CompletedTask;
    }

    private Task OnClosed(Exception? exception)
    {
        _logger.LogWarning(exception, "🔌 SignalR connection closed");
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
    }
}