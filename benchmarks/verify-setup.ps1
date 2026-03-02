# Verification Script for Source Generator Setup
# Checks if everything is configured correctly for benchmarking

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Source Generator Setup Verification" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$rootDir = Split-Path -Parent $PSScriptRoot
$mediatorCsproj = Join-Path $rootDir "src\Blazing.Mediator\Blazing.Mediator.csproj"
$sourceGenProject = Join-Path $rootDir "src\Blazing.Mediator.SourceGenerators\Blazing.Mediator.SourceGenerators.csproj"
$benchmarkCsproj = Join-Path $rootDir "benchmarks\Blazing.Mediator.Benchmarks\Blazing.Mediator.Benchmarks.csproj"

$allChecksPass = $true

# Check 1: Blazing.Mediator.csproj has SourceGen configuration
Write-Host "[1/6] Checking Blazing.Mediator.csproj configuration..." -ForegroundColor Yellow
if (Test-Path $mediatorCsproj) {
    $content = Get-Content $mediatorCsproj -Raw
    if ($content -match "Condition.*SourceGen.*USE_SOURCE_GENERATORS") {
        Write-Host "  ? SourceGen configuration found" -ForegroundColor Green
    } else {
        Write-Host "  ? SourceGen configuration NOT found" -ForegroundColor Red
        Write-Host "    Expected to find: Condition='`$(Configuration)' == 'SourceGen'" -ForegroundColor Gray
        $allChecksPass = $false
    }
} else {
    Write-Host "  ? Blazing.Mediator.csproj not found" -ForegroundColor Red
    $allChecksPass = $false
}
Write-Host ""

# Check 2: Source Generator project exists
Write-Host "[2/6] Checking SourceGenerators project..." -ForegroundColor Yellow
if (Test-Path $sourceGenProject) {
    Write-Host "  ? SourceGenerators project found" -ForegroundColor Green
    
    # Check for HandlerRegistrationGenerator
    $generatorFile = Join-Path $rootDir "src\Blazing.Mediator.SourceGenerators\Generators\HandlerRegistrationGenerator.cs"
    if (Test-Path $generatorFile) {
        Write-Host "  ? HandlerRegistrationGenerator.cs found" -ForegroundColor Green
    } else {
        Write-Host "  ? HandlerRegistrationGenerator.cs NOT found" -ForegroundColor Red
        $allChecksPass = $false
    }
} else {
    Write-Host "  ? SourceGenerators project not found" -ForegroundColor Red
    $allChecksPass = $false
}
Write-Host ""

# Check 3: Mediator.Send.cs has conditional compilation
Write-Host "[3/6] Checking Mediator.Send.cs for conditional compilation..." -ForegroundColor Yellow
$mediatorSend = Join-Path $rootDir "src\Blazing.Mediator\Mediator.Send.cs"
if (Test-Path $mediatorSend) {
    $content = Get-Content $mediatorSend -Raw
    if ($content -match "#if USE_SOURCE_GENERATORS") {
        Write-Host "  ? Conditional compilation found" -ForegroundColor Green
        if ($content -match "Generated\.RequestDispatcher\.Send") {
            Write-Host "  ? Generated dispatcher invocation found" -ForegroundColor Green
        } else {
            Write-Host "  ? Generated dispatcher invocation NOT found" -ForegroundColor Red
            $allChecksPass = $false
        }
    } else {
        Write-Host "  ? Conditional compilation NOT found" -ForegroundColor Red
        $allChecksPass = $false
    }
} else {
    Write-Host "  ? Mediator.Send.cs not found" -ForegroundColor Red
    $allChecksPass = $false
}
Write-Host ""

# Check 4: Benchmark configuration
Write-Host "[4/6] Checking benchmark configuration..." -ForegroundColor Yellow
if (Test-Path $benchmarkCsproj) {
    Write-Host "  ? Benchmark project found" -ForegroundColor Green
    
    $benchmarkConfig = Join-Path $rootDir "benchmarks\Blazing.Mediator.Benchmarks\BenchmarkConfig.cs"
    if (Test-Path $benchmarkConfig) {
        Write-Host "  ? BenchmarkConfig.cs found" -ForegroundColor Green
    } else {
        Write-Host "  ? BenchmarkConfig.cs NOT found" -ForegroundColor Red
        $allChecksPass = $false
    }
} else {
    Write-Host "  ? Benchmark project not found" -ForegroundColor Red
    $allChecksPass = $false
}
Write-Host ""

# Check 5: Try building in Release mode
Write-Host "[5/6] Test building Blazing.Mediator in Release mode..." -ForegroundColor Yellow
Write-Host "  Building..." -ForegroundColor Gray -NoNewline
$output = dotnet build $mediatorCsproj -c Release --no-incremental 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "`r  ? Release build successful" -ForegroundColor Green
} else {
    Write-Host "`r  ? Release build failed" -ForegroundColor Red
    Write-Host "  Error output:" -ForegroundColor Gray
    $output | Select-Object -Last 10 | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
    $allChecksPass = $false
}
Write-Host ""

# Check 6: Try building in SourceGen mode
Write-Host "[6/6] Test building Blazing.Mediator in SourceGen mode..." -ForegroundColor Yellow
Write-Host "  Building..." -ForegroundColor Gray -NoNewline
$output = dotnet build $mediatorCsproj -c SourceGen --no-incremental 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "`r  ? SourceGen build successful" -ForegroundColor Green
    
    # Check for generated files
    $generatedDir = Join-Path $rootDir "src\Blazing.Mediator\obj\SourceGen"
    if (Test-Path $generatedDir) {
        $generatedFiles = Get-ChildItem -Path $generatedDir -Recurse -Filter "*.g.cs" -ErrorAction SilentlyContinue
        if ($generatedFiles.Count -gt 0) {
            Write-Host "  ? Found $($generatedFiles.Count) generated file(s)" -ForegroundColor Green
            Write-Host "    Example: $($generatedFiles[0].Name)" -ForegroundColor Gray
        } else {
            Write-Host "  ? No generated files found in obj folder" -ForegroundColor Yellow
            Write-Host "    This might be OK if handlers are in consuming projects" -ForegroundColor Gray
        }
    }
} else {
    Write-Host "`r  ? SourceGen build failed" -ForegroundColor Red
    Write-Host "  Error output:" -ForegroundColor Gray
    $output | Select-Object -Last 10 | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
    $allChecksPass = $false
}
Write-Host ""

# Final summary
Write-Host "========================================" -ForegroundColor Cyan
if ($allChecksPass) {
    Write-Host "? All checks passed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Your setup is ready for benchmarking." -ForegroundColor Green
    Write-Host "Run: .\run-sourcegen-benchmark.ps1" -ForegroundColor Cyan
} else {
    Write-Host "? Some checks failed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please review the errors above and fix them before benchmarking." -ForegroundColor Yellow
    Write-Host "See: docs/source-gen/HOW_TO_BENCHMARK_SOURCEGEN.md" -ForegroundColor Cyan
}
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
