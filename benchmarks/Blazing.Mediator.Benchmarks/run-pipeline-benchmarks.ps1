# Pipeline Performance Benchmark Script
# Run this script to benchmark the performance improvements made to MiddlewarePipelineBuilder and NotificationPipelineBuilder

Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host "    Blazing.Mediator Pipeline Performance Benchmarks     " -ForegroundColor Cyan  
Write-Host "   Measuring <50ns execution target optimizations        " -ForegroundColor Cyan
Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host ""

# Change to the benchmark directory
Set-Location -Path "$PSScriptRoot"

Write-Host "Building benchmarks in Release mode..." -ForegroundColor Yellow
dotnet build -c Release --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed! Please fix compilation errors first." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Available Benchmark Suites:" -ForegroundColor Green
Write-Host "1. Pipeline Performance Benchmarks (Comprehensive)" -ForegroundColor White
Write-Host "2. Performance Bottleneck Benchmarks (Specific optimizations)" -ForegroundColor White  
Write-Host "3. Nanosecond Precision Benchmarks (<50ns target validation)" -ForegroundColor White
Write-Host "4. Run All Benchmarks" -ForegroundColor White
Write-Host ""

$choice = Read-Host "Select benchmark suite (1-4)"

switch ($choice) {
    "1" {
        Write-Host "Running Pipeline Performance Benchmarks..." -ForegroundColor Green
        dotnet run -c Release -- --filter "*PipelinePerformanceBenchmarks*" --exporters github markdown --memory
    }
    "2" {
        Write-Host "Running Performance Bottleneck Benchmarks..." -ForegroundColor Green  
        dotnet run -c Release -- --filter "*PerformanceBottleneckBenchmarks*" --exporters github markdown --memory
    }
    "3" {
        Write-Host "Running Nanosecond Precision Benchmarks..." -ForegroundColor Green
        dotnet run -c Release -- --filter "*NanosecondPrecisionBenchmarks*" --exporters github markdown --memory --job short
    }
    "4" {
        Write-Host "Running All Pipeline Benchmarks..." -ForegroundColor Green
        Write-Host "This will take several minutes to complete..." -ForegroundColor Yellow
        dotnet run -c Release -- --filter "*Pipeline*" --exporters github markdown --memory
    }
    default {
        Write-Host "Invalid selection. Running Pipeline Performance Benchmarks..." -ForegroundColor Yellow
        dotnet run -c Release -- --filter "*PipelinePerformanceBenchmarks*" --exporters github markdown --memory
    }
}

Write-Host ""
Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host "                    Benchmark Complete                     " -ForegroundColor Cyan
Write-Host "===========================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Performance Analysis Summary:" -ForegroundColor Green
Write-Host "• Assembly scanning: Should show dramatic improvement (30s -> ms)" -ForegroundColor White
Write-Host "• Sorting algorithms: O(n²) -> O(1) optimization visible" -ForegroundColor White  
Write-Host "• Memory allocations: Reduced due to LINQ elimination" -ForegroundColor White
Write-Host "• Pipeline execution: Overall faster end-to-end performance" -ForegroundColor White
Write-Host ""
Write-Host "Look for benchmark results in the console output above and in generated markdown files." -ForegroundColor Yellow