<#
.SYNOPSIS
    Local CI/CD test runner for Blazing.Mediator.

.DESCRIPTION
    Validates and executes GitHub Actions workflows locally using actionlint (static analysis)
    and act (Docker-based execution). Mirrors exactly what runs on GitHub Actions, including
    both .NET SDK versions required for the net9.0 and net10.0 multi-target builds.

.PARAMETER Mode
    dry      - Validate YAML + act dry-run only (no Docker execution, fastest)
    lint     - actionlint static analysis only (no Docker required)
    ci       - Full CI workflow execution via act (requires Docker)
    all      - lint + ci (default)

.PARAMETER Workflow
    ci       - Run only ci.yml (default for Mode=ci)
    release  - Run only release.yml
    both     - Run both workflows (sequential)

.PARAMETER Job
    Optionally run a single job by name, e.g. 'build-and-test'. Ignored for dry/lint modes.

.EXAMPLE
    .\ci-cd-test-run.ps1
    .\ci-cd-test-run.ps1 -Mode lint
    .\ci-cd-test-run.ps1 -Mode dry
    .\ci-cd-test-run.ps1 -Mode ci
    .\ci-cd-test-run.ps1 -Mode ci -Workflow release
    .\ci-cd-test-run.ps1 -Mode ci -Job build-and-test
#>

[CmdletBinding()]
param(
    [ValidateSet('dry', 'lint', 'ci', 'all')]
    [string]$Mode = 'all',

    [ValidateSet('ci', 'release', 'both')]
    [string]$Workflow = 'ci',

    [string]$Job = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Colours ────────────────────────────────────────────────────────────────────
function Write-Header  { param([string]$msg) Write-Host "`n━━━ $msg ━━━" -ForegroundColor Cyan }
function Write-Pass    { param([string]$msg) Write-Host "  ✅ $msg" -ForegroundColor Green }
function Write-Fail    { param([string]$msg) Write-Host "  ❌ $msg" -ForegroundColor Red }
function Write-Info    { param([string]$msg) Write-Host "  ℹ  $msg" -ForegroundColor DarkGray }
function Write-Warn    { param([string]$msg) Write-Host "  ⚠  $msg" -ForegroundColor Yellow }
function Write-Section { param([string]$msg) Write-Host "`n  ▶ $msg" -ForegroundColor White }

$Script:Errors   = [System.Collections.Generic.List[string]]::new()
$Script:Warnings = [System.Collections.Generic.List[string]]::new()

function Add-Error   { param([string]$msg) $Script:Errors.Add($msg);   Write-Fail   $msg }
function Add-Warning { param([string]$msg) $Script:Warnings.Add($msg); Write-Warn   $msg }

# ── Paths ──────────────────────────────────────────────────────────────────────
$RepoRoot       = $PSScriptRoot
$WorkflowDir    = Join-Path $RepoRoot '.github' 'workflows'
$CiYaml         = Join-Path $WorkflowDir 'ci.yml'
$ReleaseYaml    = Join-Path $WorkflowDir 'release.yml'
$SlnFilter      = Join-Path $RepoRoot 'Blazing.Mediator.CI.slnf'
$DbProps        = Join-Path $RepoRoot 'Directory.Build.props'
$CoreTests      = Join-Path $RepoRoot 'tests' 'Blazing.Mediator.Tests' 'Blazing.Mediator.Tests.csproj'
$ECommerceTests = Join-Path $RepoRoot 'tests' 'ECommerce.Api.Tests' 'ECommerce.Api.Tests.csproj'
$UserMgmtTests  = Join-Path $RepoRoot 'tests' 'UserManagement.Api.Tests' 'UserManagement.Api.Tests.csproj'
$StreamingTests = Join-Path $RepoRoot 'tests' 'Streaming.Api.Tests' 'Streaming.Api.Tests.csproj'
$OtelTests      = Join-Path $RepoRoot 'tests' 'OpenTelemetryExample.Tests' 'OpenTelemetryExample.Tests.csproj'
$Frameworks     = @('net9.0', 'net10.0')

# ── Tool check ─────────────────────────────────────────────────────────────────
function Test-Tool {
    param([string]$Name, [string]$InstallHint)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Add-Error "Tool '$Name' not found. Install: $InstallHint"
        return $false
    }
    return $true
}

# ══════════════════════════════════════════════════════════════════════════════
# STEP 1 — Prerequisite check
# ══════════════════════════════════════════════════════════════════════════════
Write-Header 'Prerequisite Check'

$hasDotnet      = Test-Tool 'dotnet'      'https://dotnet.microsoft.com/download'
$hasActionlint  = Test-Tool 'actionlint'  'winget install rhysd.actionlint'
$hasAct         = Test-Tool 'act'         'winget install nektos.act'

if ($hasDotnet) { Write-Pass "dotnet $(dotnet --version)" }

# Docker check (only needed for ci/all/dry modes)
$dockerAvailable = $false
if ($Mode -in @('ci', 'all', 'dry')) {
    try {
        $null = docker info 2>$null
        $dockerAvailable = $true
        Write-Pass 'Docker daemon reachable'
    } catch {
        Add-Warning 'Docker not reachable — ci/dry modes will be skipped'
    }
}

# ══════════════════════════════════════════════════════════════════════════════
# STEP 2 — actionlint static analysis
# ══════════════════════════════════════════════════════════════════════════════
if ($Mode -in @('lint', 'all')) {
    Write-Header 'YAML Static Analysis (actionlint)'

    if (-not $hasActionlint) {
        Add-Error 'actionlint not installed — skipping lint'
    } else {
        $yamlFiles = @()
        if ($Workflow -in @('ci',      'both')) { $yamlFiles += $CiYaml      }
        if ($Workflow -in @('release', 'both')) { $yamlFiles += $ReleaseYaml }

        foreach ($yaml in $yamlFiles) {
            $name = Split-Path $yaml -Leaf
            Write-Section "Linting $name"
            $out = actionlint $yaml 2>&1
            if ($LASTEXITCODE -eq 0) {
                Write-Pass "$name — no issues"
            } else {
                $out | ForEach-Object { Write-Host "    $_" -ForegroundColor Yellow }
                Add-Error "$name has actionlint violations (see above)"
            }
        }
    }
}

# ══════════════════════════════════════════════════════════════════════════════
# STEP 3 — act dry-run
# ══════════════════════════════════════════════════════════════════════════════
if ($Mode -in @('dry', 'all') -and $dockerAvailable -and $hasAct) {
    Write-Header 'Workflow Dry-Run (act -n)'

    $dryWorkflows = @()
    if ($Workflow -in @('ci',      'both')) { $dryWorkflows += @{ Name = 'CI';      File = $CiYaml      } }
    if ($Workflow -in @('release', 'both')) { $dryWorkflows += @{ Name = 'Release'; File = $ReleaseYaml } }

    foreach ($wf in $dryWorkflows) {
        Write-Section "Dry-run $($wf.Name) workflow"
        Push-Location $RepoRoot
        try {
            $out = act push --workflows $wf.File -n 2>&1
            # Filter known act Windows cache bug: upload-artifact@v4 fails to remove its own
            # .gitignore on Windows, causing a non-zero exit code even in dry-run mode.
            # Succeed if there are no real failures (excluding DRYRUN summary lines and artifact errors).
            $failed = $out | Where-Object {
                $_ -match '(FAIL|error)' -and
                $_ -notmatch 'DRYRUN' -and
                $_ -notmatch 'upload-artifact' -and
                $_ -notmatch '\.cache\\act\\actions-upload-artifact' -and
                $_ -notmatch 'The system cannot find the file specified'
            }
            if (-not $failed) {
                Write-Pass "$($wf.Name) dry-run succeeded"
            } else {
                $out | Where-Object { $_ -match '(FAIL|error|warn)' } | ForEach-Object { Write-Host "    $_" -ForegroundColor Yellow }
                Add-Error "$($wf.Name) dry-run reported issues"
            }
        } finally {
            Pop-Location
        }
    }
}

# ══════════════════════════════════════════════════════════════════════════════
# STEP 4 — Full CI execution via act
# ══════════════════════════════════════════════════════════════════════════════
if ($Mode -in @('ci', 'all') -and $dockerAvailable -and $hasAct) {
    Write-Header 'Full CI Execution (act)'

    if (-not $dockerAvailable) {
        Add-Warning 'Docker not available — skipping act execution'
    } else {
        $actWorkflows = @()
        if ($Workflow -in @('ci',      'both')) { $actWorkflows += @{ Name = 'CI';      File = $CiYaml;      Event = 'push' } }
        if ($Workflow -in @('release', 'both')) { $actWorkflows += @{ Name = 'Release'; File = $ReleaseYaml; Event = 'push' } }

        foreach ($wf in $actWorkflows) {
            Write-Section "Running $($wf.Name) workflow via act"
            Push-Location $RepoRoot
            try {
                $actArgs = @($wf.Event, '--workflows', $wf.File)
                if ($Job) { $actArgs += @('-j', $Job) }

                # Stream output, capture for analysis
                $outLines = [System.Collections.Generic.List[string]]::new()
                & act @actArgs 2>&1 | Tee-Object -Variable rawOut | ForEach-Object {
                    $outLines.Add($_)
                    # Echo lines that carry meaningful signal
                    if ($_ -match '(✅|❌|🏁|PASS|FAIL|Error|error:|warning:)') {
                        Write-Host "    $_"
                    }
                }

                # Parse results — wrap in @() to force array type (.Count fails on plain strings)
                $jobSucceeded = @($outLines | Where-Object { $_ -match '🏁.*Job succeeded' })
                $jobFailed    = @($outLines | Where-Object { $_ -match '🏁.*Job failed' })
                $testPassed   = @($outLines | Where-Object { $_ -match 'Passed!.*Failed:\s+0' })
                $testFailed   = @($outLines | Where-Object { $_ -match 'Failed!.*Failed:\s+[^0]' })

                Write-Host ''
                if ($testPassed.Count -gt 0) {
                    $testPassed | ForEach-Object { Write-Pass ($_ -replace '^\|\s*', '') }
                }
                if ($testFailed.Count -gt 0) {
                    $testFailed | ForEach-Object { Add-Error ($_ -replace '^\|\s*', '') }
                }

                # Ignore ACTIONS_RUNTIME_TOKEN artifact upload errors (known act limitation)
                $realFailures = @($jobFailed | Where-Object { $_ -notmatch 'Upload test results' })

                if ($LASTEXITCODE -eq 0 -or ($jobSucceeded.Count -gt 0 -and $realFailures.Count -eq 0)) {
                    Write-Pass "$($wf.Name) workflow — all jobs succeeded"
                } else {
                    Add-Error "$($wf.Name) workflow had job failures (see above)"
                }
            } finally {
                Pop-Location
            }
        }
    }
}

# ══════════════════════════════════════════════════════════════════════════════
# SUMMARY
# ══════════════════════════════════════════════════════════════════════════════
Write-Header 'Summary'

if ($Script:Warnings.Count -gt 0) {
    Write-Host "`n  Warnings:" -ForegroundColor Yellow
    $Script:Warnings | ForEach-Object { Write-Host "    ⚠  $_" -ForegroundColor Yellow }
}

if ($Script:Errors.Count -eq 0) {
    Write-Host "`n  ✅ All checks passed!`n" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`n  ❌ $($Script:Errors.Count) error(s) found:" -ForegroundColor Red
    $Script:Errors | ForEach-Object { Write-Host "    • $_" -ForegroundColor Red }
    Write-Host ''
    exit 1
}
