#!/usr/bin/env pwsh

Write-Host "=== Blazing.Mediator AOT Tests ===" -ForegroundColor Cyan
Write-Host ""

$testProject = "tests/Blazing.Mediator.AotTests/Blazing.Mediator.AotTests.csproj"
$outputDir = "tests/Blazing.Mediator.AotTests/bin/Release/net9.0/publish"
$logsDir = "tests/Blazing.Mediator.AotTests/logs"

# Create logs directory
New-Item -ItemType Directory -Force -Path $logsDir | Out-Null

# Step 1: Clean
Write-Host "Step 1: Cleaning..." -ForegroundColor Yellow
dotnet clean $testProject -c Release | Out-Null

# Step 2: Build with diagnostics
Write-Host "Step 2: Building with diagnostics..." -ForegroundColor Yellow
dotnet build $testProject -c Release /p:PublishAot=true -v:detailed > "$logsDir/aot-build.log" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Build failed. Check $logsDir/aot-build.log" -ForegroundColor Red
    exit 1
}

# Step 3: Publish with AOT
Write-Host "Step 3: Publishing with AOT..." -ForegroundColor Yellow
dotnet publish $testProject -c Release /p:PublishAot=true > "$logsDir/aot-publish.log" 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "? Publish failed. Check $logsDir/aot-publish.log" -ForegroundColor Red
    exit 1
}

# Step 4: Analyze trimming warnings
Write-Host "Step 4: Analyzing trimming warnings..." -ForegroundColor Yellow
dotnet publish $testProject -c Release /p:PublishAot=true /p:SuppressTrimAnalysisWarnings=false > "$logsDir/aot-trim-warnings.log" 2>&1

# Step 5: Run AOT binary
Write-Host "Step 5: Running AOT-compiled binary..." -ForegroundColor Yellow
$exePath = if ($IsWindows) { "$outputDir/Blazing.Mediator.AotTests.exe" } else { "$outputDir/Blazing.Mediator.AotTests" }

if (Test-Path $exePath) {
    & $exePath > "$logsDir/aot-runtime.log" 2>&1
    $exitCode = $LASTEXITCODE
    
    if ($exitCode -eq 0) {
        Write-Host "? AOT tests passed!" -ForegroundColor Green
        Get-Content "$logsDir/aot-runtime.log"
    } else {
        Write-Host "? AOT tests failed with exit code: $exitCode" -ForegroundColor Red
        Get-Content "$logsDir/aot-runtime.log"
        exit $exitCode
    }
} else {
    Write-Host "? AOT binary not found: $exePath" -ForegroundColor Red
    exit 1
}

# Step 6: Analyze results
Write-Host ""
Write-Host "=== Analysis ===" -ForegroundColor Cyan

Write-Host "Build warnings:" -ForegroundColor Yellow
Select-String -Path "$logsDir/aot-build.log" -Pattern "warning" | Select-Object -First 10

Write-Host ""
Write-Host "Trimming warnings:" -ForegroundColor Yellow
Select-String -Path "$logsDir/aot-trim-warnings.log" -Pattern "warning IL" | Select-Object -First 10

Write-Host ""
Write-Host "Logs saved to: $logsDir" -ForegroundColor Cyan
