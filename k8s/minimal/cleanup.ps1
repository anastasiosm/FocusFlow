# FocusFlow Kubernetes Cleanup Script

Write-Host "Cleaning up FocusFlow from Kubernetes..." -ForegroundColor Yellow
Write-Host "==========================================="

# Delete the entire namespace (this removes everything)
kubectl delete namespace focusflow

Write-Host ""
Write-Host "Cleanup complete!" -ForegroundColor Green
Write-Host ""
Write-Host "All FocusFlow resources have been removed from the cluster." -ForegroundColor Gray
