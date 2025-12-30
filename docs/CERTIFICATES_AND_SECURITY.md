# Certificates and Security Architecture in FocusFlow

This document explains how security mechanisms, including HTTPS certificates, Data Protection, Authentication tokens, and Real-time Communication Security are implemented and configured across the FocusFlow application (Local Development, Docker, and Testing).

## 1. HTTPS Certificates (Transport Layer Security)

These certificates ensure encrypted communication between the browser and the application (providing the lock icon in the browser).

### Local Development
*   **Tooling:** We use the `dotnet dev-certs` tool.
*   **Setup Script:** The `scripts/setup-dev-certs.ps1` script:
    1.  Generates a self-signed development certificate (`aspnetapp.pfx`).
    2.  Saves it to the `certs/` directory in the repository root.
    3.  Trusts the certificate in the developer's OS certificate store.

### Docker Environment
Containers do not have native access to the host's trusted certificate store.
*   **Configuration:** In `docker-compose.yml`, the `certs/` directory is **bind-mounted** into the Blazor container:
    ```yaml
    volumes:
      - ./certs:/https:ro
    ```
*   **Kestrel Setup:** The application server (Kestrel) is configured via environment variables to use this mounted certificate:
    ```yaml
    environment:
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=${CERT_PASSWORD:-MyPfxPassword123!}
    ```
*   **API Service:** The Web API container runs on HTTP (port 8080) internally within the secure Docker network and is not directly exposed to the internet, so it does not require an HTTPS certificate in this configuration.

## 2. Data Protection Keys (Browser Storage Encryption)

ASP.NET Core uses the **Data Protection API** to encrypt sensitive data stored in the browser, such as data saved via `ProtectedLocalStorage`.

### The Challenge
By default, Data Protection keys are stored in memory or generated per-instance. If a container restarts, keys are lost, causing encrypted browser storage to become unreadable and users to be logged out.

### The Solution in FocusFlow
*   **Code Configuration:** In `Program.cs`, Data Protection is configured to persist keys to a specific file system location:
    ```csharp
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo("/tmp/dataprotection-keys"))
        .SetApplicationName("FocusFlow");
    ```
*   **Docker Persistence:** In `docker-compose.yml`, this directory is mapped to a volume to persist keys across container restarts:
    ```yaml
    volumes:
      - ./dataprotection:/dataprotection-keys:rw
    ```
*   **Usage in FocusFlow:** The `TokenProvider` uses `ProtectedLocalStorage` to securely store JWT tokens in the browser:
    ```csharp
    // Encrypted storage in browser
    await _protectedLocalStorage.SetAsync(TOKEN_KEY, token);
    ```
*   **E2E Testing:** During E2E tests (`E2ETestEnvironment.cs`), a temporary shared directory is created on the host and mounted to both the API and Client containers. This ensures they share the same encryption keyring, allowing successful authentication flows during tests.

## 3. Authentication Tokens (JWT Signing)

For API authentication, FocusFlow uses **JSON Web Tokens (JWT)**.

*   **Mechanism:** Instead of certificates (asymmetric encryption), a **Symmetric Secret Key** (HMACSHA256) is used.
*   **Configuration:** The key is a long string defined in environment variables (e.g., `JwtSettings__SecretKey`).
*   **Trust:** Both the issuing authority (API) and the consumer (if validating locally) share this secret key to sign and verify tokens.

## 4. Enhanced JWT Authentication Flow

The flow ensures stateless and secure authentication between the Blazor Client (Server-side rendered) and the Web API, with improved token management for real-time scenarios.

### 4.1 Login Process
1.  **Login:**
    *   User enters credentials.
    *   Client POSTs to `/api/auth/login`.
    *   API validates user and issues an **Access Token** (JWT).

### 4.2 Enhanced Token Storage
The `TokenProvider` has been enhanced with dual storage strategy:

*   **Static Cache:** Uses `ConcurrentDictionary<string, string?>` for immediate access across scoped instances
*   **Persistent Storage:** Uses `ProtectedLocalStorage` for browser refresh persistence
*   **Thread-Safe Initialization:** Lazy loading with locks to prevent race conditions

```csharp
public class TokenProvider : ITokenProvider
{
    private static readonly ConcurrentDictionary<string, string?> _tokenCache = new();
    private readonly ProtectedLocalStorage _protectedLocalStorage;
    
    public async Task<string?> GetTokenAsync()
    {
        // Try cache first, then initialize from storage if needed
        if (_tokenCache.TryGetValue(CACHE_KEY, out var cachedToken))
            return cachedToken;
            
        await InitializeAsync(); // Lazy initialization
        return _tokenCache.TryGetValue(CACHE_KEY, out var token) ? token : null;
    }
}
```

### 4.3 Authentication Handler Integration
The `BlazorAuthenticationHandler` ensures proper JWT authentication for all HTTP requests:

```csharp
public class BlazorAuthenticationHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _tokenProvider.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
```

### 4.4 Authenticated Requests Flow
1.  **Request Interception:** The `BlazorAuthenticationHandler` intercepts all outgoing HTTP requests
2.  **Token Retrieval:** Retrieves JWT from `TokenProvider` (cache or storage)
3.  **Header Injection:** Adds `Authorization: Bearer <token>` header
4.  **API Validation:** API validates JWT signature and claims
5.  **Response:** Returns data or 401 Unauthorized

## 5. SignalR Real-time Communication Security

FocusFlow implements secure real-time communication using SignalR with JWT authentication.

### 5.1 SignalR Hub Security
```csharp
[Authorize] // Requires valid JWT token
public class TasksHub : Hub
{
    public async Task JoinProjectAsync(string projectId)
    {
        // Only authenticated users can join project groups
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Project_{projectId}");
    }
}
```

### 5.2 SignalR Client Authentication
The SignalR client is configured to use JWT tokens for authentication:

```csharp
public class SignalRService : ISignalRService
{
    public async Task StartAsync()
    {
        var token = await _tokenProvider.GetTokenAsync();
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                // Add JWT token to connection
                options.AccessTokenProvider = () => Task.FromResult(token);
                
                // Configure for development (accept self-signed certificates)
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
            .WithAutomaticReconnect()
            .Build();
    }
}
```

### 5.3 Project-Based Security Groups
SignalR uses project-based groups to ensure users only receive notifications for projects they have access to:

*   **Group Membership:** Users join groups like `Project_{projectId}` when viewing a project
*   **Targeted Notifications:** Events are sent only to relevant project groups
*   **Automatic Cleanup:** Users leave groups when navigating away

### 5.4 SignalR Security Flow
1.  **Connection:** Client connects to SignalR hub with JWT token
2.  **Authentication:** Hub validates JWT and establishes authenticated connection
3.  **Group Management:** Client joins/leaves project groups based on navigation
4.  **Event Publishing:** Server publishes events to specific project groups
5.  **Authorization:** Only authenticated users in relevant groups receive notifications

## 6. Secrets Management (Dev vs. Prod)

Security relies heavily on keeping keys and passwords secret.

*   **Local Development:** We use the **.NET User Secrets** tool (`dotnet user-secrets`). This stores sensitive data (like DB connection strings) in a file in the user's profile directory, outside the git repository.
*   **Docker/CI:** We inject secrets via Environment Variables.
    *   *Current State:* In `docker-compose.yml`, secrets are visible for convenience.
    *   *Production Rule:* **NEVER** commit `docker-compose.yml` with hardcoded real passwords to source control. In production, use a secret manager (like Azure Key Vault, AWS Secrets Manager, or Docker Swarm/K8s Secrets) to inject these values at runtime.

## 7. Input Validation (Defense in Depth)

FocusFlow employs a layered validation strategy to prevent Injection attacks and ensure data integrity.

*   **Client-Side:** Immediate feedback using FluentValidation in Blazor forms. This is for User Experience (UX), not security.
*   **Server-Side:** The API re-validates **all** incoming DTOs using FluentValidation. This is the **Security Boundary**. We never trust data solely because the client said it was valid.

## 8. Security Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Browser (HTTPS)                          │
│  ┌─────────────────┐    ┌─────────────────────────────────┐ │
│  │ Blazor Client   │    │ SignalR Client                  │ │
│  │ (JWT in         │◄──►│ (JWT Authentication)            │ │
│  │ ProtectedStorage│    │                                 │ │
│  └─────────────────┘    └─────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
                    │                           │
                    ▼ HTTPS + JWT              ▼ WSS + JWT
┌─────────────────────────────────────────────────────────────┐
│                Docker Network (Internal)                    │
│  ┌─────────────────┐    ┌─────────────────────────────────┐ │
│  │ Web API         │    │ SignalR Hub                     │ │
│  │ (JWT Validation)│    │ ([Authorize] + Project Groups)  │ │
│  │                 │    │                                 │ │
│  └─────────────────┘    └─────────────────────────────────┘ │
│                    │                           │            │
│                    ▼                           ▼            │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │              PostgreSQL Database                        │ │
│  │              (Encrypted at Rest)                        │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## 9. Production Readiness Checklist

Before deploying FocusFlow to a production environment, ensure the following:

### Authentication & Authorization
*   [ ] **Replace JWT Keys:** Change `JwtSettings__SecretKey` to a strong, randomly generated string (min 32 chars) and keep it private.
*   [ ] **Token Expiration:** Configure appropriate JWT expiration times (e.g., 1 hour for access tokens).
*   [ ] **Refresh Tokens:** Consider implementing refresh tokens for longer sessions.

### Transport Security
*   [ ] **HSTS & HTTPS:** Ensure the API sits behind a Reverse Proxy (Nginx, Traefik, IIS) that handles HTTPS termination and enforces HSTS (Strict-Transport-Security).
*   [ ] **Secure Storage:** Ensure `ProtectedLocalStorage` data is properly encrypted with persistent Data Protection keys.
*   [ ] **SignalR WSS:** Ensure SignalR connections use secure WebSockets (WSS) in production.

### Database Security
*   [ ] **Database User:** Create a specific database user with **Least Privilege** access (don't use the `postgres` superuser).
*   [ ] **Connection Encryption:** Enable SSL/TLS for database connections.
*   [ ] **Data Protection Keys:** Ensure data protection keys are stored securely and backed up.

### SignalR Security
*   [ ] **Hub Authorization:** Verify all SignalR hubs have proper `[Authorize]` attributes.
*   [ ] **Group Security:** Implement proper authorization checks for project group membership.
*   [ ] **Connection Limits:** Configure appropriate connection limits and timeouts.
*   [ ] **CORS Configuration:** Properly configure CORS for SignalR endpoints.

### Monitoring & Logging
*   [ ] **Security Logging:** Log authentication failures, unauthorized access attempts.
*   [ ] **SignalR Monitoring:** Monitor connection counts, message rates, and errors.
*   [ ] **Token Monitoring:** Log token validation failures and suspicious patterns.