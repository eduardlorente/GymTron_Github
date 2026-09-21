$CurrentDirectory = Get-Location
Set-Location "$CurrentDirectory"

# Ensure UTF-8 encoding
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Load environment variables from .env file if it exists
if (Test-Path ".env") {
    Get-Content ".env" | ForEach-Object {
        if ($_ -match "^\s*([^#][^=]+)=(.*)$") {
            $key = $matches[1].Trim()
            $value = $matches[2].Trim()
            if (-not [Environment]::GetEnvironmentVariable($key, "Process")) {
                [Environment]::SetEnvironmentVariable($key, $value, "Process")
            }
        }
    }
}

$MYSQL_ROOT_PASSWORD = [Environment]::GetEnvironmentVariable("MYSQL_ROOT_PASSWORD", "Process")
if ([string]::IsNullOrWhiteSpace($MYSQL_ROOT_PASSWORD)) {
    Write-Host "Error: MYSQL_ROOT_PASSWORD is not set. Configure it in .env or environment." -ForegroundColor Red
    exit 1
}

$JWT_SECRET_KEY = [Environment]::GetEnvironmentVariable("JWT_SECRET_KEY", "Process")
if ([string]::IsNullOrWhiteSpace($JWT_SECRET_KEY) -or [System.Text.Encoding]::UTF8.GetByteCount($JWT_SECRET_KEY) -lt 32) {
    Write-Host "Error: JWT_SECRET_KEY is not set or shorter than 32 bytes (256 bits). Configure it in .env or environment." -ForegroundColor Red
    exit 1
}

$API_PORT = [Environment]::GetEnvironmentVariable("API_PORT", "Process")
if ([string]::IsNullOrWhiteSpace($API_PORT)) {
    $API_PORT = "5000"
}

Write-Host "Building and starting GymTron containers (Database & API)..." -ForegroundColor Green
docker compose up -d --build

if ($LASTEXITCODE -eq 0) {
    Write-Host "GymTron environment is running!" -ForegroundColor Green
    Write-Host "API: http://localhost:$API_PORT" -ForegroundColor Cyan
    Write-Host "Scalar UI: http://localhost:$API_PORT/scalar/v1" -ForegroundColor Cyan
    Write-Host "OpenAPI docs: http://localhost:$API_PORT/openapi/v1.json" -ForegroundColor Cyan
    Write-Host "To follow logs: docker compose logs -f" -ForegroundColor Yellow
} else {
    Write-Host "Failed to start Docker Compose." -ForegroundColor Red
    exit $LASTEXITCODE
}
