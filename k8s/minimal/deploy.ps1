# FocusFlow Minimal Kubernetes Deployment Script
# This script deploys FocusFlow to Kubernetes with minimal configuration

Write-Host "Deploying FocusFlow to Kubernetes (Minimal Setup)" -ForegroundColor Cyan
Write-Host "==================================================" -ForegroundColor Cyan

# Check if kubectl is available
if (!(Get-Command kubectl -ErrorAction SilentlyContinue)) {
    Write-Host "kubectl not found. Please install kubectl first." -ForegroundColor Red
    exit 1
}

# Check if images exist locally
Write-Host ""
Write-Host "Checking Docker images..." -ForegroundColor Yellow
$apiImage = docker images focusflow-focusflow-api:latest --format "{{.Repository}}"
$blazorImage = docker images focusflow-focusflow-blazor:latest --format "{{.Repository}}"

if (!$apiImage) {
    Write-Host "Warning: focusflow-focusflow-api:latest image not found locally" -ForegroundColor Yellow
    Write-Host "   Build it with: docker build -f src/FocusFlow.WebApi/Dockerfile -t focusflow-focusflow-api:latest ." -ForegroundColor Gray
}

if (!$blazorImage) {
    Write-Host "Warning: focusflow-focusflow-blazor:latest image not found locally" -ForegroundColor Yellow
    Write-Host "   Build it with: docker build -f src/FocusFlow.BlazorApp/Dockerfile -t focusflow-focusflow-blazor:latest ." -ForegroundColor Gray
}

Write-Host ""
Write-Host "Applying Kubernetes manifests..." -ForegroundColor Yellow

# Apply manifests in order
kubectl apply -f namespace.yaml
Write-Host "Namespace created" -ForegroundColor Green

kubectl apply -f secrets.yaml
Write-Host "Secrets created" -ForegroundColor Green

kubectl apply -f configmap.yaml
Write-Host "ConfigMap created" -ForegroundColor Green

kubectl apply -f postgres.yaml
Write-Host "PostgreSQL deployed" -ForegroundColor Green

Write-Host ""
Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l app=focusflow-postgres -n focusflow --timeout=120s

kubectl apply -f api.yaml
Write-Host "API deployed" -ForegroundColor Green

kubectl apply -f blazor.yaml
Write-Host "Blazor app deployed" -ForegroundColor Green

Write-Host ""
Write-Host "Waiting for all pods to be ready..." -ForegroundColor Yellow
kubectl wait --for=condition=ready pod -l app=focusflow-api -n focusflow --timeout=120s
kubectl wait --for=condition=ready pod -l app=focusflow-blazor -n focusflow --timeout=120s

Write-Host ""
Write-Host "Deployment complete!" -ForegroundColor Green
Write-Host ""
Write-Host "Current status:" -ForegroundColor Cyan
kubectl get pods -n focusflow

Write-Host ""
Write-Host "Access the application:" -ForegroundColor Cyan
Write-Host "   API:    kubectl port-forward -n focusflow service/focusflow-api 8080:8080" -ForegroundColor White
Write-Host "   Blazor: kubectl port-forward -n focusflow service/focusflow-blazor 8081:8081" -ForegroundColor White
Write-Host ""
Write-Host "View logs:" -ForegroundColor Cyan
Write-Host "   API:    kubectl logs -f deployment/focusflow-api -n focusflow" -ForegroundColor White
Write-Host "   Blazor: kubectl logs -f deployment/focusflow-blazor -n focusflow" -ForegroundColor White
Write-Host ""
Write-Host "Health checks:" -ForegroundColor Cyan
Write-Host "   API:    curl http://localhost:8080/health (after port-forward)" -ForegroundColor White
Write-Host "   Blazor: curl http://localhost:8081/health (after port-forward)" -ForegroundColor White
