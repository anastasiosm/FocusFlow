# FocusFlow - Minimal Kubernetes Setup

**Purpose:** Simplified Kubernetes deployment for demos and local testing (Windows only).

---

## What's Included

| Type | Files |
|------|-------|
| **Manifests** | 6 Kubernetes YAML files |
| **Scripts** | 2 PowerShell scripts |
| **Docs** | This README |

**Total: 9 files (~13 KB)**

---

## Prerequisites

1. **Kubernetes cluster**
   - Docker Desktop with Kubernetes enabled (recommended)
   - Or Minikube
   - Or any cloud Kubernetes

2. **kubectl** installed and configured

3. **Docker** for building images

---

## Quick Start

### Step 1: Build Docker Images

From the repository root:

```powershell
# Build API
docker build -f src/FocusFlow.WebApi/Dockerfile -t focusflow-api:latest .

# Build Blazor UI
docker build -f src/FocusFlow.BlazorApp/Dockerfile -t focusflow-blazor:latest .
```

> **For Minikube users:** Load images into the cluster:
> ```powershell
> minikube image load focusflow-api:latest
> minikube image load focusflow-blazor:latest
> ```

### Step 2: Deploy to Kubernetes

```powershell
cd k8s/minimal
.\deploy.ps1
```

The script will:
1. Create the `focusflow` namespace
2. Apply secrets and configmap
3. Deploy PostgreSQL
4. Deploy API
5. Deploy Blazor UI
6. Wait for all pods to be ready

### Step 3: Access the Application

Open **two separate PowerShell windows**:

```powershell
# Window 1: API port-forward
kubectl port-forward -n focusflow service/focusflow-api 8080:8080

# Window 2: Blazor UI port-forward
kubectl port-forward -n focusflow service/focusflow-blazor 8081:8081
```

Then open in your browser:
- **Blazor UI**: http://localhost:8081
- **API Swagger**: http://localhost:8080/swagger

---

## Verification

### Check Pod Status
```powershell
kubectl get pods -n focusflow
```

**Expected output:**
```
NAME                                 READY   STATUS    RESTARTS   AGE
focusflow-api-xxxxxxxxxx-xxxxx       1/1     Running   0          2m
focusflow-blazor-xxxxxxxxxx-xxxxx    1/1     Running   0          2m
focusflow-postgres-xxxxxxxxxx-xxxxx  1/1     Running   0          2m
```

All pods should show `1/1` in the READY column and `Running` status.

### Check Health Endpoints

After port-forwarding the API:

```powershell
# General health
curl http://localhost:8080/health

# Liveness probe
curl http://localhost:8080/health/live

# Readiness probe
curl http://localhost:8080/health/ready
```

### View Logs

```powershell
# API logs
kubectl logs -f deployment/focusflow-api -n focusflow

# Blazor logs
kubectl logs -f deployment/focusflow-blazor -n focusflow

# PostgreSQL logs
kubectl logs -f deployment/focusflow-postgres -n focusflow

# View last 50 lines
kubectl logs --tail=50 deployment/focusflow-api -n focusflow
```

---

## Configuration

### Default Secrets (secrets.yaml)

**WARNING: Demo values only - change for any real use!**

- **PostgreSQL password**: `demo123`
- **JWT secret**: `demo-jwt-secret-key-minimum-32-chars-long`

To customize, edit `secrets.yaml` before deploying.

### Environment Variables (configmap.yaml)

- `ASPNETCORE_ENVIRONMENT`: `Development`
- `POSTGRES_HOST`: `focusflow-postgres`
- `POSTGRES_DB`: `focusflow`
- `POSTGRES_USER`: `focusflow`

---

## Health Checks

Both API and Blazor include Kubernetes health probes:

### Liveness Probe (`/health/live`)
- **Purpose**: Is the application alive?
- **Check**: Basic application health (no external dependencies)
- **Failure action**: Pod restart
- **Timing**: Initial delay 30s, check every 10s

### Readiness Probe (`/health/ready`)
- **Purpose**: Is the application ready to serve traffic?
- **Checks**: Database connectivity, API dependencies
- **Failure action**: Pod removed from Service (no traffic)
- **Timing**: Initial delay 10s, check every 5s

---

## Troubleshooting

### Pods Not Starting

```powershell
# Check status
kubectl get pods -n focusflow

# Get detailed info
kubectl describe pod <pod-name> -n focusflow

# Check logs
kubectl logs <pod-name> -n focusflow
```

### ImagePullBackOff Error

This means Kubernetes can't find the Docker images.

```powershell
# Verify images exist locally
docker images | findstr focusflow

# For Minikube, load images
minikube image load focusflow-api:latest
minikube image load focusflow-blazor:latest

# For Kind
kind load docker-image focusflow-api:latest
kind load docker-image focusflow-blazor:latest
```

### CrashLoopBackOff Error

The pod is starting but crashing repeatedly.

```powershell
# Check current logs
kubectl logs <pod-name> -n focusflow

# Check previous container logs
kubectl logs <pod-name> -n focusflow --previous

# Check events
kubectl get events -n focusflow --sort-by='.lastTimestamp'
```

### Health Check Failures

```powershell
# Check health probe configuration
kubectl describe pod <pod-name> -n focusflow

# Test health endpoint manually
kubectl exec -it deployment/focusflow-api -n focusflow -- curl http://localhost:8080/health

# Check application logs
kubectl logs <pod-name> -n focusflow
```

### Database Connection Issues

```powershell
# Check PostgreSQL is running
kubectl get pod -l app=focusflow-postgres -n focusflow

# Test database connection
kubectl exec -it deployment/focusflow-postgres -n focusflow -- pg_isready -U focusflow

# Connect to database
kubectl exec -it deployment/focusflow-postgres -n focusflow -- psql -U focusflow -d focusflow

# Test from API pod
kubectl exec -it deployment/focusflow-api -n focusflow -- ping focusflow-postgres
```

---

## Cleanup

Remove all FocusFlow resources:

```powershell
cd k8s/minimal
.\cleanup.ps1
```

Or manually:
```powershell
kubectl delete namespace focusflow
```

> **NOTE:** This deletes **all data** including the database.

---

## Files Structure

```
k8s/minimal/
??? namespace.yaml       # Namespace definition
??? secrets.yaml         # Passwords & JWT secret
??? configmap.yaml       # Environment configuration
??? postgres.yaml        # PostgreSQL database
??? api.yaml             # FocusFlow API
??? blazor.yaml          # FocusFlow Blazor UI
??? deploy.ps1           # One-command deployment
??? cleanup.ps1          # Quick cleanup
??? README.md            # This file
```

**Total: 9 files (~13 KB)**

---

## What's Included

- PostgreSQL database (ephemeral storage)
- FocusFlow API with health checks
- FocusFlow Blazor UI with health checks
- One-command deployment script
- Quick cleanup script

---

## What's NOT Included

This is intentionally minimal for demos:

- **NO Ingress** - Use `kubectl port-forward` instead
- **NO Persistent Volumes** - Data lost on pod restart (faster startup)
- **NO Resource limits** - Simpler manifests
- **NO Monitoring/Logging** - Keep it minimal
- **NO SSL/TLS** - Not needed for local demos
- **NO Autoscaling** - Single replica only

**For production features**, see the full setup in `k8s/` directory.

---

## Common Commands Reference

### Deployment
```powershell
.\deploy.ps1                                    # Deploy everything
kubectl get pods -n focusflow                   # Check status
kubectl get pods -n focusflow -w                # Watch pods (real-time)
```

### Access
```powershell
kubectl port-forward -n focusflow service/focusflow-api 8080:8080
kubectl port-forward -n focusflow service/focusflow-blazor 8081:8081
```

### Logs
```powershell
kubectl logs -f deployment/focusflow-api -n focusflow
kubectl logs -f deployment/focusflow-blazor -n focusflow
kubectl logs --tail=50 deployment/focusflow-api -n focusflow
```

### Debug
```powershell
kubectl describe pod <pod-name> -n focusflow
kubectl exec -it deployment/focusflow-api -n focusflow -- /bin/bash
kubectl get events -n focusflow --sort-by='.lastTimestamp'
```

### Restart
```powershell
kubectl rollout restart deployment/focusflow-api -n focusflow
kubectl delete pod <pod-name> -n focusflow
```

### Cleanup
```powershell
.\cleanup.ps1                                   # Quick cleanup
kubectl delete namespace focusflow              # Manual cleanup
```

---

## Use Cases

This minimal setup is **perfect** for:

- Quick demos and presentations
- Local development and testing
- Learning Kubernetes basics
- Proof of concept (POC)
- CI/CD testing pipelines

This minimal setup is **NOT suitable** for:

- Production deployments
- Data persistence requirements
- External access needs (without port-forward)
- Multi-environment setups

---

## Learning Resources

- **Full Production Setup**: [k8s/minimal/README.md](../minimal/README.md)
- **Docker Compose Setup**: [docker-compose.yml](../../docker-compose.yml)
- **Kubernetes Docs**: https://kubernetes.io/docs/
- **kubectl Cheat Sheet**: https://kubernetes.io/docs/reference/kubectl/cheatsheet/

---

## FAQ

**Q: Do I need a cloud Kubernetes cluster?**  
A: No! Works with Docker Desktop, Minikube, or Kind locally.

**Q: Will my data persist if pods restart?**  
A: No, this uses ephemeral storage for simplicity. For persistence, use the full setup.

**Q: Can I use this in production?**  
A: Not recommended. This is optimized for demos/testing. Use the full setup for production.

**Q: How long does deployment take?**  
A: Less than 5 minutes from start to finish.

**Q: Why can't I access the app without port-forward?**  
A: No Ingress is configured to keep it minimal. Port-forward is simpler for demos.

**Q: The images show ImagePullBackOff, what do I do?**  
A: For local clusters (Minikube/Kind), load images with `minikube image load` or `kind load docker-image`.

---

## Getting Help

1. Check the [Troubleshooting](#troubleshooting) section above
2. View pod logs: `kubectl logs <pod-name> -n focusflow`
3. Describe pod: `kubectl describe pod <pod-name> -n focusflow`
4. Check events: `kubectl get events -n focusflow --sort-by='.lastTimestamp'`

---

## Summary

You have a **minimal, Windows-focused, demo-ready** Kubernetes setup:

- **9 files total** (~13 KB)
- **One-command deployment** (`.\deploy.ps1`)
- **Production-quality health checks**
- **Less than 5 minutes** to deploy
- **Perfect for demos**

**Ready to start? Run:**
```powershell
cd k8s/minimal
.\deploy.ps1
```

---

*Happy deploying!*
