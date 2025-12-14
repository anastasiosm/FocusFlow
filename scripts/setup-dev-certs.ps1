#!/usr/bin/env pwsh
# Setup development certificates for Docker HTTPS (exports to repo ./certs and user profile)

Write-Host "Setting up development certificates..." -ForegroundColor Green

$repoCertsDir = Resolve-Path "$PSScriptRoot/../certs"
$userHttpsDir = Join-Path $env:USERPROFILE ".aspnet/https"

# Ensure directories exist
New-Item -ItemType Directory -Force -Path $repoCertsDir | Out-Null
New-Item -ItemType Directory -Force -Path $userHttpsDir | Out-Null

# Password: prefer environment variable CERT_PASSWORD, otherwise fallback to default dev password
$certPassword = $env:CERT_PASSWORD
if ([string]::IsNullOrWhiteSpace($certPassword)) {
    $certPassword = "MyPfxPassword123!"
}

$repoPfxPath = Join-Path $repoCertsDir "aspnetapp.pfx"
$userPfxPath = Join-Path $userHttpsDir "aspnetapp.pfx"

Write-Host "Generating development certificate..." -ForegroundColor Yellow
# Export to user profile (so dotnet tooling still works) and to repo certs for Docker
dotnet dev-certs https -ep $userPfxPath -p $certPassword

if ($LASTEXITCODE -ne 0) {
    Write-Host "✖ Failed to generate certificate in user profile" -ForegroundColor Red
    exit 1
}

# Copy the generated PFX into the repository certs folder (overwrite)
Copy-Item -Path $userPfxPath -Destination $repoPfxPath -Force

Write-Host "✔ Certificate exported to:" -ForegroundColor Green
Write-Host "  $userPfxPath" -ForegroundColor Cyan
Write-Host "  $repoPfxPath" -ForegroundColor Cyan

# Trust the certificate on Windows (interactive)
if ($IsWindows) {
    Write-Host "Trusting certificate (Windows)..." -ForegroundColor Yellow
    dotnet dev-certs https --trust
} else {
    Write-Host "Note: Certificate trust on macOS/Linux may require manual steps or elevated permissions." -ForegroundColor Yellow
}

Write-Host "`n✔ Setup complete! You can now run: docker-compose up" -ForegroundColor Green