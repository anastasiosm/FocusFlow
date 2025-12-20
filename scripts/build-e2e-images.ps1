#!/usr/bin/env pwsh

# Build Docker images for E2E testing with Testcontainers

Write-Host "🐳 Building Docker images for E2E testing..." -ForegroundColor Cyan

# Build API image
Write-Host "📦 Building API image (focusflow-api:test)..." -ForegroundColor Yellow
docker build -f src/FocusFlow.WebApi/Dockerfile -t focusflow-api:test .
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to build API image" -ForegroundColor Red
    exit 1
}

# Build Blazor client image  
Write-Host "📦 Building Blazor client image (focusflow-client:test)..." -ForegroundColor Yellow
docker build -f src/FocusFlow.BlazorApp/Dockerfile -t focusflow-client:test .
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Failed to build Blazor client image" -ForegroundColor Red
    exit 1
}

Write-Host "✅ All Docker images built successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Available test images:" -ForegroundColor Cyan
docker images | Select-String "focusflow.*test"

Write-Host ""
Write-Host "🧪 Ready to run E2E tests with:" -ForegroundColor Green
Write-Host "   dotnet test tests/FocusFlow.E2E.Tests/" -ForegroundColor White