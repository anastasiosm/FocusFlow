#!/usr/bin/env pwsh
# Complete E2E test runner with Docker management
# Usage: .\run-e2e-tests.ps1 [-SkipBuild] [-KeepRunning] [-Filter "TestName"]

param(
    [switch]$SkipBuild,
    [switch]$KeepRunning,
    [string]$Filter = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot
$solutionRoot = Resolve-Path "$scriptDir/.."

Write-Host "`n=== FocusFlow E2E Test Runner ===" -ForegroundColor Cyan
Write-Host "Solution root: $solutionRoot`n" -ForegroundColor Gray

# Change to solution root
Push-Location $solutionRoot

try {
    # 1. Stop any existing containers
    Write-Host "[1/6] Stopping existing containers..." -ForegroundColor Yellow
    docker-compose down -v 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ? No existing containers to stop" -ForegroundColor Gray
    } else {
        Write-Host "  ? Containers stopped" -ForegroundColor Green
    }

    # 2. Build images if needed
    if (-not $SkipBuild) {
        Write-Host "`n[2/6] Building Docker images..." -ForegroundColor Yellow
        docker-compose build --progress=plain
        if ($LASTEXITCODE -ne 0) {
            throw "Docker build failed"
        }
        Write-Host "  ? Images built successfully" -ForegroundColor Green
    } else {
        Write-Host "`n[2/6] Skipping build (--SkipBuild flag)" -ForegroundColor Gray
    }

    # 3. Start containers
    Write-Host "`n[3/6] Starting Docker containers..." -ForegroundColor Yellow
    docker-compose up -d
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to start containers"
    }
    Write-Host "  ? Containers started" -ForegroundColor Green

    # 4. Wait for services to be ready
    Write-Host "`n[4/6] Waiting for services to be healthy..." -ForegroundColor Yellow
    
    # Increase maximum wait to allow migrations, seeding and app startup
    $maxWait = 180
    $elapsed = 0
    $interval = 3

    while ($elapsed -lt $maxWait) {
        try {
            $pgHealth = docker inspect --format='{{.State.Health.Status}}' focusflow-postgres-db-1 2>$null
            $apiStatus = docker inspect --format='{{.State.Status}}' focusflow-focusflow-api-1 2>$null
            $blazorStatus = docker inspect --format='{{.State.Status}}' focusflow-focusflow-blazor-1 2>$null

            if ($pgHealth -eq "healthy" -and $apiStatus -eq "running" -and $blazorStatus -eq "running") {
                Write-Host "  ? PostgreSQL: healthy" -ForegroundColor Green
                Write-Host "  ? API: running" -ForegroundColor Green
                Write-Host "  ? Blazor: running" -ForegroundColor Green
                break
            }

            Write-Host "  ? Waiting... ($elapsed/$maxWait seconds)" -ForegroundColor Gray
        }
        catch {
            Write-Host "  ? Waiting for containers... ($elapsed/$maxWait seconds)" -ForegroundColor Gray
        }

        Start-Sleep -Seconds $interval
        $elapsed += $interval
    }

    if ($elapsed -ge $maxWait) {
        Write-Host "`n? Services failed to start within $maxWait seconds" -ForegroundColor Red
        docker-compose logs --tail=50
        throw "Timeout waiting for services"
    }

    Write-Host "  ? Waiting for Blazor to initialize..." -ForegroundColor Gray
    Start-Sleep -Seconds 10

    # 5. Test connectivity with a more robust readiness probe (check both Blazor and API)
    Write-Host "`n[5/6] Testing application connectivity..." -ForegroundColor Yellow

    $connectivityTimeout = 180
    $connectivityStart = Get-Date
    $blazorReady = $false
    $apiReady = $false

    while (((Get-Date) - $connectivityStart).TotalSeconds -lt $connectivityTimeout) {
        try {
            $bResp = Invoke-WebRequest -Uri "http://localhost:5050" -Method Get -TimeoutSec 5 -ErrorAction Stop
            if ($bResp.StatusCode -ge 200 -and $bResp.StatusCode -lt 500) {
                Write-Host "  ? Blazor app responding (Status: $($bResp.StatusCode))" -ForegroundColor Green
                $blazorReady = $true
            }
        }
        catch {
            # Blazor is ready even if it returns 4xx (e.g., 404 on root) as long as it responds
            if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
                $statusCode = [int]$_.Exception.Response.StatusCode
                if ($statusCode -ge 200 -and $statusCode -lt 500) {
                    Write-Host "  ? Blazor app responding (Status: $statusCode)" -ForegroundColor Green
                    $blazorReady = $true
                } else {
                    Write-Host "  ? Blazor not ready yet..." -ForegroundColor Gray
                }
            } else {
                Write-Host "  ? Blazor not ready yet..." -ForegroundColor Gray
            }
        }

        try {
            $aResp = Invoke-WebRequest -Uri "http://localhost:8080/api" -Method Get -TimeoutSec 5 -ErrorAction Stop
            if ($aResp.StatusCode -ge 200 -and $aResp.StatusCode -lt 500) {
                Write-Host "  ? API responding (Status: $($aResp.StatusCode))" -ForegroundColor Green
                $apiReady = $true
            }
        }
        catch {
            # API is ready even if it returns 4xx (e.g., 404 on /api) as long as it responds
            if ($_.Exception.Response -and $_.Exception.Response.StatusCode) {
                $statusCode = [int]$_.Exception.Response.StatusCode
                if ($statusCode -ge 200 -and $statusCode -lt 500) {
                    Write-Host "  ? API responding (Status: $statusCode)" -ForegroundColor Green
                    $apiReady = $true
                } else {
                    Write-Host "  ? API not ready yet..." -ForegroundColor Gray
                }
            } else {
                Write-Host "  ? API not ready yet..." -ForegroundColor Gray
            }
        }

        if ($blazorReady -and $apiReady) {
            break
        }

        # Print recent container logs periodically to help debug slow startup
        $secondsSinceStart = ((Get-Date) - $connectivityStart).TotalSeconds
        if ([int]$secondsSinceStart % 15 -eq 0) {
            Write-Host "  ? Tailing recent logs for focusflow-blazor and focusflow-api..." -ForegroundColor DarkYellow
            docker-compose logs --tail 50 focusflow-blazor focusflow-api
        }

        Start-Sleep -Seconds $interval
    }

    if (-not ($blazorReady -and $apiReady)) {
        Write-Host "  ? Warning: Could not reach Blazor and/or API within timeout" -ForegroundColor Yellow
    }

    # 6. Run E2E tests
    Write-Host "`n[6/6] Running E2E tests..." -ForegroundColor Yellow
    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Gray

    $testArgs = @(
        "test",
        "tests/FocusFlow.E2E.Tests",
        "--logger", "console;verbosity=detailed"
    )

    if ($Filter) {
        $testArgs += "--filter"
        $testArgs += $Filter
    }

    & dotnet @testArgs
    $testExitCode = $LASTEXITCODE

    Write-Host "???????????????????????????????????????????????????" -ForegroundColor Gray

    if ($testExitCode -eq 0) {
        Write-Host "`n? All E2E tests passed!" -ForegroundColor Green
    } else {
        Write-Host "`n? Some E2E tests failed!" -ForegroundColor Red
        docker-compose logs --tail=30 focusflow-blazor
    }

    if (-not $KeepRunning) {
        Write-Host "`nStopping Docker containers..." -ForegroundColor Yellow
        docker-compose down
    } else {
        Write-Host "`nContainers still running:" -ForegroundColor Cyan
        Write-Host "  Blazor: http://localhost:5050" -ForegroundColor Green
        Write-Host "  API: http://localhost:8080" -ForegroundColor Green
        Write-Host "`nTo stop: docker-compose down" -ForegroundColor Gray
    }

    exit $testExitCode
}
catch {
    Write-Host "`n? Error: $_" -ForegroundColor Red
    docker-compose down 2>$null
    exit 1
}
finally {
    Pop-Location
}
