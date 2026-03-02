# Validation Benchmark Runner
# Tests if source generation is actually working

Write-Host ""
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "BLAZING.MEDIATOR SOURCE GENERATION VALIDATION" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""

$ErrorActionPreference = "Stop"
$projectPath = "benchmarks\Blazing.Mediator.Benchmarks.Temp\Blazing.Mediator.Benchmarks.Temp.csproj"

function Run-ValidationBenchmark {
    param(
        [string]$Configuration,
        [string]$Description,
        [ConsoleColor]$Color = [ConsoleColor]::Yellow
    )
    
    Write-Host ""
    Write-Host "==========================================================" -ForegroundColor $Color
    Write-Host "Testing: $Description" -ForegroundColor $Color
    Write-Host "Configuration: $Configuration" -ForegroundColor $Color
    Write-Host "==========================================================" -ForegroundColor $Color
    Write-Host ""
    
    # Clean to ensure fresh build
    Write-Host "Cleaning previous build..." -ForegroundColor Gray
    dotnet clean $projectPath -c $Configuration --nologo -v q
    
    # Build
    Write-Host "Building in $Configuration mode..." -ForegroundColor Cyan
    dotnet build $projectPath -c $Configuration --no-incremental
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Build failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "? Build successful" -ForegroundColor Green
    
    # Check for generated files if SourceGen
    if ($Configuration -eq "SourceGen") {
        Write-Host ""
        Write-Host "Checking for generated files..." -ForegroundColor Cyan
        $generatedDir = "benchmarks\Blazing.Mediator.Benchmarks.Temp\obj\SourceGen\net10.0\generated"
        
        if (Test-Path $generatedDir) {
            $genFiles = Get-ChildItem -Path $generatedDir -Recurse -Filter "*.g.cs" -ErrorAction SilentlyContinue
            if ($genFiles) {
                Write-Host "? Found $($genFiles.Count) generated file(s):" -ForegroundColor Green
                foreach ($file in $genFiles) {
                    Write-Host "  - $($file.Name)" -ForegroundColor Gray
                }
            } else {
                Write-Host "? No .g.cs files found in generated directory" -ForegroundColor Yellow
            }
        } else {
            Write-Host "? Generated directory not found: $generatedDir" -ForegroundColor Yellow
        }
    }
    
    Write-Host ""
    Write-Host "Running benchmark..." -ForegroundColor Cyan
    Write-Host ""
    
    # Run benchmark
    dotnet run --project $projectPath -c $Configuration --no-build
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host ""
        Write-Host "ERROR: Benchmark failed!" -ForegroundColor Red
        exit 1
    }
    
    Write-Host ""
    Write-Host "? $Description complete" -ForegroundColor Green
    Write-Host ""
}

# Test 1: Reflection (baseline)
Run-ValidationBenchmark -Configuration "Release" -Description "Reflection (Baseline)" -Color Yellow

Write-Host ""
Write-Host "Press any key to continue to Source Generation test..." -ForegroundColor Cyan
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
Write-Host ""

# Test 2: Source Generation
Run-ValidationBenchmark -Configuration "SourceGen" -Description "Source Generation (Optimized)" -Color Green

# Summary
Write-Host ""
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "VALIDATION COMPLETE" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Compare the results above:" -ForegroundColor Yellow
Write-Host "  - Memory allocation should be LOWER with Source Generation" -ForegroundColor Yellow
Write-Host "  - Execution time should be FASTER with Source Generation" -ForegroundColor Yellow
Write-Host "  - Check BenchmarkDotNet.Artifacts for detailed HTML reports" -ForegroundColor Yellow
Write-Host ""
Write-Host "Expected improvements:" -ForegroundColor Cyan
Write-Host "  • 15-35% faster execution" -ForegroundColor Gray
Write-Host "  • 30-50% less memory allocation" -ForegroundColor Gray
Write-Host ""

# Open results directory
$artifactsDir = "BenchmarkDotNet.Artifacts"
if (Test-Path $artifactsDir) {
    Write-Host "Results saved to: $artifactsDir" -ForegroundColor Cyan
    
    $openResults = Read-Host "Open results directory? [Y/n]"
    if ([string]::IsNullOrWhiteSpace($openResults) -or $openResults -eq "Y" -or $openResults -eq "y") {
        Start-Process $artifactsDir
    }
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
Write-Host ""
