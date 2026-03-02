# Blazing.Mediator Source Generator Benchmark Script
# This script runs benchmarks comparing reflection-based vs source generation performance

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Blazing.Mediator Benchmark Runner" -ForegroundColor Cyan
Write-Host "Reflection vs Source Generation" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"

# Get the root directory (where the .sln file is)
$rootDir = Split-Path -Parent $PSScriptRoot
$benchmarkProject = Join-Path $rootDir "benchmarks\Blazing.Mediator.Benchmarks\Blazing.Mediator.Benchmarks.csproj"
$resultsDir = Join-Path $rootDir "BenchmarkDotNet.Artifacts"

Write-Host "Root Directory: $rootDir" -ForegroundColor Gray
Write-Host "Benchmark Project: $benchmarkProject" -ForegroundColor Gray
Write-Host ""

# Function to run benchmarks
function Run-Benchmark {
    param(
        [string]$ConfigName,
        [string]$Description
    )
    
    Write-Host "=======================================" -ForegroundColor Yellow
    Write-Host "Running: $Description" -ForegroundColor Yellow
    Write-Host "Configuration: $ConfigName" -ForegroundColor Yellow
    Write-Host "=======================================" -ForegroundColor Yellow
    Write-Host ""
    
    # Build the benchmark project with the specific configuration
    Write-Host "Step 1: Building benchmark project ($ConfigName)..." -ForegroundColor Cyan
    dotnet build $benchmarkProject -c $ConfigName --no-incremental --framework net10.0
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Failed to build benchmark project in $ConfigName configuration" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "? Build successful" -ForegroundColor Green
    Write-Host ""
    
    # Run the benchmarks
    Write-Host "Step 2: Running benchmarks (this may take several minutes)..." -ForegroundColor Cyan
    dotnet run --project $benchmarkProject -c $ConfigName --no-build --framework net10.0
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Benchmark run failed" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "? Benchmark completed successfully" -ForegroundColor Green
    Write-Host ""
}

# Check if user wants to run both or just one
$choice = Read-Host "Run benchmarks for: [1] Reflection only, [2] SourceGen only, [3] Both (comparison) [default: 3]"
if ([string]::IsNullOrWhiteSpace($choice)) {
    $choice = "3"
}

Write-Host ""

switch ($choice) {
    "1" {
        Run-Benchmark -ConfigName "Release" -Description "Reflection-based (baseline)"
    }
    "2" {
        Run-Benchmark -ConfigName "SourceGen" -Description "Source Generation (optimized)"
    }
    "3" {
        Write-Host "Running FULL comparison benchmark..." -ForegroundColor Magenta
        Write-Host "This will run both configurations for accurate comparison" -ForegroundColor Magenta
        Write-Host ""
        
        Run-Benchmark -ConfigName "Release" -Description "Reflection-based (baseline)"
        
        Write-Host ""
        Write-Host "=======================================" -ForegroundColor Magenta
        Write-Host "Preparing for SourceGen benchmark..." -ForegroundColor Magenta
        Write-Host "=======================================" -ForegroundColor Magenta
        Write-Host ""
        
        Run-Benchmark -ConfigName "SourceGen" -Description "Source Generation (optimized)"
    }
    default {
        Write-Host "Invalid choice. Exiting." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "All benchmarks completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""

if (Test-Path $resultsDir) {
    Write-Host "Results saved to: $resultsDir" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Latest results:" -ForegroundColor Yellow
    Get-ChildItem -Path $resultsDir -Recurse -Filter "*.html" | 
        Sort-Object LastWriteTime -Descending | 
        Select-Object -First 1 | 
        ForEach-Object { 
            Write-Host "  HTML Report: $($_.FullName)" -ForegroundColor White
            
            # Try to open the HTML report
            $openReport = Read-Host "Open HTML report in browser? [Y/n]"
            if ([string]::IsNullOrWhiteSpace($openReport) -or $openReport -eq "Y" -or $openReport -eq "y") {
                Start-Process $_.FullName
            }
        }
}

Write-Host ""
Write-Host "Tip: Compare the 'Reflection-NET9' vs 'SourceGen-NET9' results" -ForegroundColor Yellow
Write-Host "Expected improvements: 15-35% faster, 30-50% less allocation" -ForegroundColor Yellow
Write-Host ""
