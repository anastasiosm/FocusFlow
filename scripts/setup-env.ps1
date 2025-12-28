# FocusFlow Environment Setup Script
# This script helps developers set up their local .env file

Write-Host "🚀 FocusFlow Environment Setup" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# Check if .env already exists
if (Test-Path ".env") {
    Write-Host "⚠️  .env file already exists!" -ForegroundColor Yellow
    $overwrite = Read-Host "Do you want to overwrite it? (y/N)"
    if ($overwrite -ne "y" -and $overwrite -ne "Y") {
        Write-Host "❌ Setup cancelled." -ForegroundColor Red
        exit 1
    }
}

# Check if .env.example exists
if (-not (Test-Path ".env.example")) {
    Write-Host "❌ .env.example file not found!" -ForegroundColor Red
    Write-Host "Make sure you're running this from the project root." -ForegroundColor Red
    exit 1
}

# Copy .env.example to .env
try {
    Copy-Item ".env.example" ".env" -Force
    Write-Host "✅ Created .env file from .env.example" -ForegroundColor Green
} catch {
    Write-Host "❌ Failed to create .env file: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "🎉 Environment setup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Current Configuration:" -ForegroundColor Cyan
Write-Host "• Database: FocusFlowDb_Dev (Development)" -ForegroundColor White
Write-Host "• API URL: http://localhost:8080" -ForegroundColor White  
Write-Host "• Blazor URL: http://localhost:5050" -ForegroundColor White
Write-Host ""
Write-Host "📝 Next steps:" -ForegroundColor Cyan
Write-Host "1. Review and customize your .env file if needed" -ForegroundColor White
Write-Host "2. Run: docker-compose up --build" -ForegroundColor White
Write-Host "3. Access the app at: http://localhost:5050" -ForegroundColor White
Write-Host ""
Write-Host "🔒 Security Note:" -ForegroundColor Yellow
Write-Host "The .env file contains sensitive information and is excluded from Git." -ForegroundColor White
Write-Host "Never commit .env files to version control!" -ForegroundColor White
Write-Host ""
Write-Host "💡 Pro Tip:" -ForegroundColor Magenta
Write-Host "For different environments, create separate .env files:" -ForegroundColor White
Write-Host "• .env.development (local dev)" -ForegroundColor White
Write-Host "• .env.staging (staging environment)" -ForegroundColor White
Write-Host "• .env.production (production environment)" -ForegroundColor White