Write-Host "==========================================" -ForegroundColor Cyan
Write-Host " Launching Luminy.React Interactive Demo " -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Cyan

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptDir

if (-not (Test-Path "node_modules")) {
    Write-Host "Installing dependencies..." -ForegroundColor Yellow
    npm install
}

Write-Host "Starting Vite development server..." -ForegroundColor Green
npm run dev -- --open
