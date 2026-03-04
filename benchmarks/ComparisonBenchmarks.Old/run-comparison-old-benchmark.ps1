<#
.SYNOPSIS
    Runs the pre-optimisation baseline comparison benchmark against the old
    Blazing.Mediator v2.0.1 (master branch) and saves results to a dated folder.

.DESCRIPTION
    Builds the ComparisonBenchmarks.Old project in Release configuration, runs all
    benchmarks (old Blazing.Mediator v2.0.1 reflection-based dispatch vs
    martinothamar/Mediator and MediatR), and saves both the console transcript and
    the BenchmarkDotNet Markdown / CSV artefacts to a timestamped results folder.

    Results are saved to:
        benchmarks/ComparisonBenchmarks.Old/results/<timestamp>/

    Compare these numbers directly against ComparisonBenchmarks (optimised) results
    in benchmarks/ComparisonBenchmarks/results/ to quantify the source-generator
    overhaul gains.

.NOTES
    Run from the repository root (c:\wip\NET10\zzz) or from the script's
    own directory.  Set HUSKY=0 to prevent Husky git-hook restoration from
    running inside the martinothamar/Mediator project reference.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Resolve paths ──────────────────────────────────────────────────────────────
$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ScriptDir 'ComparisonBenchmarks.Old.csproj'
$Timestamp   = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
$ResultsDir  = Join-Path $ScriptDir "results\$Timestamp"

New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null

$TranscriptFile = Join-Path $ResultsDir 'benchmark-console-output.txt'
$SummaryFile    = Join-Path $ResultsDir 'benchmark-summary.md'
$ArtifactsDir   = Join-Path $ResultsDir 'BenchmarkDotNet.Artifacts'

Write-Host ''
Write-Host '============================================================' -ForegroundColor Cyan
Write-Host '  Mediator Comparison Benchmark — Pre-Optimisation Baseline (Old Blazing.Mediator v2.0.1)' -ForegroundColor Cyan
Write-Host '============================================================' -ForegroundColor Cyan
Write-Host ''
Write-Host "  Project    : $ProjectFile"
Write-Host "  Results    : $ResultsDir"
Write-Host "  Started at : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host ''

# ── Prevent Husky from running inside the Mediator project reference ───────────
$env:HUSKY = '0'

# ── Restore & build ──────────────────────────────────────────────────────────
Write-Host '-- Restoring packages ...' -ForegroundColor DarkCyan
dotnet restore $ProjectFile --nologo
if ($LASTEXITCODE -ne 0) { throw "Restore failed (exit $LASTEXITCODE)" }

Write-Host ''
Write-Host '-- Building in Release ...' -ForegroundColor DarkCyan
dotnet build $ProjectFile -c Release --nologo --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)" }

# ── Run benchmarks ─────────────────────────────────────────────────────────────
Write-Host ''
Write-Host '-- Running benchmarks (this will take several minutes) ...' -ForegroundColor DarkCyan
Write-Host "   Console output will be saved to: $TranscriptFile"
Write-Host ''

$BdnArgs = @(
    '--filter', '*',
    '--artifacts', $ArtifactsDir,
    '--exporters', 'GitHub', 'CSV'
)

dotnet run --project $ProjectFile -c Release --no-build -- @BdnArgs 2>&1 |
    Tee-Object -FilePath $TranscriptFile

if ($LASTEXITCODE -ne 0) { throw "Benchmark run failed (exit $LASTEXITCODE)" }

# ── Copy BenchmarkDotNet artefacts ─────────────────────────────────────────────
$BdnReport = Get-ChildItem -Path $ArtifactsDir -Filter '*-report-github.md' -Recurse |
    Select-Object -First 1

if ($BdnReport) {
    $ReportContent = Get-Content $BdnReport.FullName -Raw

    # Build the summary markdown file
    $SummaryContent = @"
# Mediator Comparison Benchmark — Pre-Optimisation Baseline (Old Blazing.Mediator v2.0.1)
Generated : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Project   : ComparisonBenchmarks.Old
Config    : Release / net10.0

> **Note:** Old Blazing.Mediator v2.0.1 (master branch) is running in **reflection-based dispatch mode**.
> There is NO source generator. On every Send/Publish/SendStream call the Mediator resolves
> IRequestHandler<T,R> via IServiceProvider.GetServices() on an open-generic type, then dispatches
> via MethodInfo.Invoke(). All optional features are disabled (no telemetry, no statistics, no logging).
> Old Blazing.Mediator IMediator is Scoped, pre-resolved from a long-lived scope.

## $($BdnReport.Name)

$ReportContent
"@

    Set-Content -Path $SummaryFile -Value $SummaryContent -Encoding UTF8
    Write-Host "  Summary saved : $SummaryFile" -ForegroundColor Green
}

Write-Host ''
Write-Host '============================================================' -ForegroundColor Green
Write-Host '  Benchmark complete!' -ForegroundColor Green
Write-Host "  Results folder : $ResultsDir" -ForegroundColor Green
Write-Host '============================================================' -ForegroundColor Green
Write-Host ''
