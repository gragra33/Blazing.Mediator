#!/usr/bin/env pwsh

$logsDir = "tests/Blazing.Mediator.AotTests/logs"
$reportFile = "$logsDir/aot-analysis-report.md"

@"
# AOT Compatibility Analysis Report

**Generated**: $(Get-Date)

## Summary

"@ | Out-File $reportFile

# Count warnings
$buildWarnings = Select-String -Path "$logsDir/aot-build.log" -Pattern "warning" -AllMatches -ErrorAction SilentlyContinue
$trimWarnings = Select-String -Path "$logsDir/aot-trim-warnings.log" -Pattern "warning IL" -AllMatches -ErrorAction SilentlyContinue

@"
- **Build Warnings**: $($buildWarnings.Count)
- **Trim Warnings**: $($trimWarnings.Count)

## Build Warnings

``````
"@ | Out-File $reportFile -Append

$buildWarnings | Select-Object -First 50 | Out-File $reportFile -Append

@"
``````

## Trim Warnings (IL2XXX)

These warnings indicate reflection usage that may not be compatible with AOT:

``````
"@ | Out-File $reportFile -Append

$trimWarnings | Select-Object -First 50 | Out-File $reportFile -Append

@"
``````

## Reflection Usage Analysis

Searching for problematic patterns in Blazing.Mediator...

### GetType() Calls
``````
"@ | Out-File $reportFile -Append

Get-ChildItem -Path "src/Blazing.Mediator" -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue | 
    Select-String -Pattern "\.GetType\(\)" | 
    Select-Object -First 20 | 
    Out-File $reportFile -Append

@"
``````

### MakeGenericType() Calls
``````
"@ | Out-File $reportFile -Append

Get-ChildItem -Path "src/Blazing.Mediator" -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue | 
    Select-String -Pattern "MakeGenericType\(" | 
    Select-Object -First 20 | 
    Out-File $reportFile -Append

@"
``````

### Assembly.GetTypes() Calls
``````
"@ | Out-File $reportFile -Append

Get-ChildItem -Path "src/Blazing.Mediator" -Filter "*.cs" -Recurse -ErrorAction SilentlyContinue | 
    Select-String -Pattern "\.GetTypes\(\)" | 
    Select-Object -First 20 | 
    Out-File $reportFile -Append

@"
``````

## Recommendations

Based on the analysis above, the following actions are recommended:

1. **Address IL2XXX Warnings**: Each IL2XXX warning indicates code that may fail at runtime in AOT scenarios.

2. **Replace GetType() Calls**: Use source-generated type information where possible.

3. **Eliminate MakeGenericType()**: Replace with compile-time generic instantiation.

4. **Replace Assembly Scanning**: Use source-generated type catalogs.

## Next Steps

1. Review each warning in detail
2. Categorize as Hot Path vs Cold Path
3. Prioritize fixes for hot path issues
4. Create remediation plan
"@ | Out-File $reportFile -Append

Write-Host "Analysis complete. Report saved to: $reportFile" -ForegroundColor Green
Get-Content $reportFile
