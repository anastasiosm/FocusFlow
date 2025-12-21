# Certificates and Security Architecture in FocusFlow

This document explains how security mechanisms, including HTTPS certificates, Data Protection, and Authentication tokens, are implemented and configured across the FocusFlow application (Local Development, Docker, and Testing).

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

## 2. Data Protection Keys (Cookie & State Encryption)

ASP.NET Core uses the **Data Protection API** to encrypt sensitive data at rest, such as authentication cookies, anti-forgery tokens, and protected session state.

### The Challenge
By default, keys are stored in memory or generated per-instance. If a container restarts or if multiple containers (API & Blazor) need to share encrypted state (like cookies), keys are lost or mismatched, causing users to be logged out.

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
*   **E2E Testing:** During E2E tests (`E2ETestEnvironment.cs`), a temporary shared directory is created on the host and mounted to both the API and Client containers. This ensures they share the same encryption keyring, allowing successful authentication flows during tests.

## 3. Authentication Tokens (JWT Signing)

For API authentication, FocusFlow uses **JSON Web Tokens (JWT)**.

*   **Mechanism:** Instead of certificates (asymmetric encryption), a **Symmetric Secret Key** (HMACSHA256) is used.
*   **Configuration:** The key is a long string defined in environment variables (e.g., `JwtSettings__SecretKey`).
*   **Trust:** Both the issuing authority (API) and the consumer (if validating locally) share this secret key to sign and verify tokens.

## 4. JWT Authentication Flow

The flow ensures stateless and secure authentication between the Blazor Client (Server-side rendered) and the Web API.

1.  **Login:**
    *   User enters credentials.
    *   Client POSTs to `/api/auth/login`.
    *   API validates user and issues an **Access Token** (JWT).

2.  **Storage (Client-Side):**
    *   The `TokenProvider` stores the JWT in **Memory** (ConcurrentDictionary) for immediate access by the server-side circuit.
    *   It also persists the token to **LocalStorage** (via `Blazored.LocalStorage`) so the user remains logged in after a page refresh.

3.  **Authenticated Requests:**
    *   The `AuthHeaderHandler` (a DelegatingHandler) intercepts all outgoing HTTP requests to the API.
    *   It retrieves the token from `TokenProvider`.
    *   It appends the header: `Authorization: Bearer <token>`.

4.  **Validation (Server-Side):**
    *   The API's JWT Middleware intercepts the request.
    *   It verifies the signature using the **Secret Key**.
    *   It checks expiration (`exp` claim).
    *   If valid, it sets the `User` principal; otherwise, returns 401 Unauthorized.

## 5. Secrets Management (Dev vs. Prod)

Security relies heavily on keeping keys and passwords secret.

*   **Local Development:** We use the **.NET User Secrets** tool (`dotnet user-secrets`). This stores sensitive data (like DB connection strings) in a file in the user's profile directory, outside the git repository.
*   **Docker/CI:** We inject secrets via Environment Variables.
    *   *Current State:* In `docker-compose.yml`, secrets are visible for convenience.
    *   *Production Rule:* **NEVER** commit `docker-compose.yml` with hardcoded real passwords to source control. In production, use a secret manager (like Azure Key Vault, AWS Secrets Manager, or Docker Swarm/K8s Secrets) to inject these values at runtime.

## 6. Input Validation (Defense in Depth)

FocusFlow employs a layered validation strategy to prevent Injection attacks and ensure data integrity.

*   **Client-Side:** Immediate feedback using FluentValidation in Blazor forms. This is for User Experience (UX), not security.
*   **Server-Side:** The API re-validates **all** incoming DTOs using FluentValidation. This is the **Security Boundary**. We never trust data solely because the client said it was valid.

## 7. Production Readiness Checklist

Before deploying FocusFlow to a production environment, ensure the following:

*   [ ] **Replace JWT Keys:** Change `JwtSettings__SecretKey` to a strong, randomly generated string (min 32 chars) and keep it private.
*   [ ] **Database User:** Create a specific database user with **Least Privilege** access (don't use the `postgres` superuser).
*   [ ] **HSTS & HTTPS:** Ensure the API sits behind a Reverse Proxy (Nginx, Traefik, IIS) that handles HTTPS termination and enforces HSTS (Strict-Transport-Security).
*   [ ] **Secure Cookies:** In `Program.cs`, ensure `Cookie.SecurePolicy` is set to `Always` (handled automatically by ASP.NET Core when HTTPS is detected).