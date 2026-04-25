# WebSocket Demo - Local Server Runner
# This script starts a local HTTP server to serve the demo properly

Write-Host "🚀 ReviewFilms WebSocket Demo Server" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if running from correct directory
if (-not (Test-Path "websocket-demo.html")) {
    Write-Host "❌ Error: websocket-demo.html not found in current directory" -ForegroundColor Red
    Write-Host "Please run this script from the 'docs' directory" -ForegroundColor Yellow
    exit 1
}

Write-Host "📂 Current directory: $(Get-Location)" -ForegroundColor Green
Write-Host ""

# Try Python first (most reliable)
$python = Get-Command python -ErrorAction SilentlyContinue
if ($python) {
    Write-Host "🐍 Using Python HTTP Server" -ForegroundColor Green
    Write-Host ""
    Write-Host "Starting server on port 8000..." -ForegroundColor Cyan
    Write-Host "📬 Demo URL: http://localhost:8000/websocket-demo.html" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
    Write-Host ""
    python -m http.server 8000
    exit 0
}

# Fallback: Try Node.js http-server
$npx = Get-Command npx -ErrorAction SilentlyContinue
if ($npx) {
    Write-Host "📦 Using Node.js http-server" -ForegroundColor Green
    Write-Host ""
    Write-Host "Starting server on port 8000..." -ForegroundColor Cyan
    Write-Host "📬 Demo URL: http://localhost:8000/websocket-demo.html" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Press Ctrl+C to stop the server" -ForegroundColor Yellow
    Write-Host ""
    npx http-server --port 8000
    exit 0
}

# Fallback: Instructions for manual setup
Write-Host "⚠️  No automatic server found. Here are manual options:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Option 1: VS Code Live Server (Recommended)" -ForegroundColor Cyan
Write-Host "  1. Install 'Live Server' extension in VS Code" -ForegroundColor Gray
Write-Host "  2. Right-click websocket-demo.html" -ForegroundColor Gray
Write-Host "  3. Select 'Open with Live Server'" -ForegroundColor Gray
Write-Host ""
Write-Host "Option 2: Python HTTP Server" -ForegroundColor Cyan
Write-Host "  python -m http.server 8000 --directory ." -ForegroundColor Gray
Write-Host "  Then open: http://localhost:8000/websocket-demo.html" -ForegroundColor Gray
Write-Host ""
Write-Host "Option 3: Node.js http-server" -ForegroundColor Cyan
Write-Host "  npx http-server --port 8000" -ForegroundColor Gray
Write-Host "  Then open: http://localhost:8000/websocket-demo.html" -ForegroundColor Gray
Write-Host ""
Write-Host "Option 4: IIS Express (if installed)" -ForegroundColor Cyan
Write-Host "  iisexpress /path:.\ /port:8000" -ForegroundColor Gray
Write-Host ""
