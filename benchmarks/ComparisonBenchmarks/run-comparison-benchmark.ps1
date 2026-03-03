<#
.SYNOPSIS
    Runs the mediator comparison benchmark and saves the output to dated files.

.DESCRIPTION
    Builds the ComparisonBenchmarks project in Release configuration, runs all
    benchmarks, and captures both the console transcript and the BenchmarkDotNet
    Markdown / CSV artefacts to a timestamped results folder so that the
    pre-optimisation baseline is preserved as a permanent reference.

    Results are saved to:
        benchmarks/ComparisonBenchmarks/results/<timestamp>/

    BenchmarkDotNet artefacts (including the full Markdown table) are also
    written there so the numbers can be pasted directly into the plan document.

.NOTES
    Run from the repository root (c:\wip\NET10\zzz) or from the script's
    own directory.  Set HUSKY=0 to prevent Husky git-hook restoration from
    running inside the martinothamar/Mediator project reference.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Resolve paths ──────────────────────────────────────────────────────────────
$ScriptDir   = Split-Path -Parent $MyInvocation.MyCommand.Path
$ProjectFile = Join-Path $ScriptDir 'ComparisonBenchmarks.csproj'
$Timestamp   = Get-Date -Format 'yyyy-MM-dd_HH-mm-ss'
$ResultsDir  = Join-Path $ScriptDir "results\$Timestamp"

New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null

$TranscriptFile = Join-Path $ResultsDir 'benchmark-console-output.txt'
$SummaryFile    = Join-Path $ResultsDir 'benchmark-summary.md'
$ArtifactsDir   = Join-Path $ResultsDir 'BenchmarkDotNet.Artifacts'

Write-Host ''
Write-Host '============================================================' -ForegroundColor Cyan
Write-Host '  Mediator Comparison Benchmark — Source-Generator Post-Optimisation' -ForegroundColor Cyan
Write-Host '============================================================' -ForegroundColor Cyan
Write-Host ''
Write-Host "  Project    : $ProjectFile"
Write-Host "  Results    : $ResultsDir"
Write-Host "  Started at : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
Write-Host ''

# ── Prevent Husky from running inside the Mediator project reference ───────────
$env:HUSKY = '0'

# ── Restore & build (fast sanity check before running) ───────────────────────
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

# BenchmarkDotNet arguments:
#   --filter *              run every benchmark class in the assembly
#   --artifacts <path>      override the default artifacts output directory
#   --exporters            GitHub (GitHub-flavoured Markdown), CSV

$BdnArgs = @(
    '--filter', '*',
    '--artifacts', $ArtifactsDir,
    '--exporters', 'GitHub', 'CSV'
)

# Run and tee to the transcript file simultaneously
dotnet run --project $ProjectFile -c Release --no-build -- @BdnArgs 2>&1 |
    Tee-Object -FilePath $TranscriptFile

$ExitCode = $LASTEXITCODE

if ($ExitCode -ne 0) {
    Write-Warning "Benchmark exited with code $ExitCode — check $TranscriptFile for details."
}

# ── Locate and consolidate Markdown results ────────────────────────────────────
$MdFiles = Get-ChildItem -Path $ArtifactsDir -Recurse -Filter '*.md' -ErrorAction SilentlyContinue |
    Where-Object { $_.Name -notlike 'combined*' }

if ($MdFiles) {
    Write-Host ''
    Write-Host '-- Consolidating Markdown result tables ...' -ForegroundColor DarkCyan

    $Header = @"
# Mediator Comparison Benchmark — Source-Generator Post-Optimisation
Generated : $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')
Project   : ComparisonBenchmarks
Config    : Release / net10.0

> **Note:** Blazing.Mediator is running in **source-generated dispatch mode**.
> `ContainerMetadata` is a Singleton whose `Init(sp)` call pre-resolves handlers
> and middleware chains from the root `IServiceProvider` at startup.
> `Mediator.GetDispatcher()` resolves `MediatorDispatcherBase` on first call and
> caches it via `FastLazyValue` — subsequent dispatches pay only one `Volatile.Read`.
> All optional features are disabled (no telemetry, no statistics, no logging).
> Blazing.Mediator IMediator is Scoped, pre-resolved from a long-lived scope.

"@

    $Header | Set-Content -Path $SummaryFile -Encoding UTF8

    foreach ($MdFile in $MdFiles) {
        $RelPath = $MdFile.FullName.Replace($ArtifactsDir, '').TrimStart('\','/')
        "## $RelPath`n" | Add-Content -Path $SummaryFile -Encoding UTF8
        Get-Content $MdFile.FullName | Add-Content -Path $SummaryFile -Encoding UTF8
        "`n" | Add-Content -Path $SummaryFile -Encoding UTF8
    }

    Write-Host "   Summary saved to: $SummaryFile" -ForegroundColor Green
}
else {
    Write-Warning "No Markdown result files found in $ArtifactsDir"
}

# ── Done ───────────────────────────────────────────────────────────────────────
Write-Host ''
Write-Host '============================================================' -ForegroundColor Green
Write-Host '  Benchmark complete'                                          -ForegroundColor Green
Write-Host '============================================================' -ForegroundColor Green
Write-Host ''
Write-Host "  Results folder : $ResultsDir"
Write-Host "  Console log    : $TranscriptFile"
if (Test-Path $SummaryFile) {
    Write-Host "  Summary tables : $SummaryFile"
}
Write-Host ''
Write-Host 'Paste the tables from the summary file into sourcegen-update-plan.md.' -ForegroundColor Yellow
Write-Host ''
