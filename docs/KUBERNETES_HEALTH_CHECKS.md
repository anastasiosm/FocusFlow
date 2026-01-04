# Kubernetes Health Checks (minimal)

Purpose: brief description of the health checks used by FocusFlow for Kubernetes deployments.

## What is checked (summary)
- Database connection (API) — ensures the database responds.
- API self-check — basic application health check.
- Blazor self-check + API dependency (Blazor checks API availability).

## Endpoints
- `/health` — overall health (all registered checks)
- `/health/ready` — readiness (dependency checks, e.g. DB, external APIs)
- `/health/live` — liveness (self checks only)

## Suggested Kubernetes probes (examples)
Readiness probe (example):

```yaml
readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
  timeoutSeconds: 3
  failureThreshold: 3
```

Liveness probe (example):

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10
  timeoutSeconds: 5
  failureThreshold: 3
```

## Implementation pointers (code)
Full implementations live in the repository — view the source if you need class details:

- `src/FocusFlow.WebApi/HealthChecks/DatabaseHealthCheck.cs` — custom database health check (API)
- `src/FocusFlow.BlazorApp/Services/ApiHealthCheck.cs` — API health check (Blazor)

In the source you'll find details about timeouts, logging and the shape of returned data.

## Notes
- This document is intentionally minimal — it does not include full class implementations.
- For troubleshooting, check pod logs and query endpoints using `kubectl port-forward` and `curl`.

Local check example:

```bash
kubectl port-forward -n focusflow service/focusflow-api 8080:8080
curl http://localhost:8080/health/ready
```

---
Minimal documentation retained; full implementations are in the source files listed above.
