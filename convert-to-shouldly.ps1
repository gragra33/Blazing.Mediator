# PowerShell script to convert FluentAssertions to Shouldly syntax
param(
    [string]$TargetDirectory = "tests"
)

Write-Host "Converting FluentAssertions to Shouldly in $TargetDirectory..." -ForegroundColor Green

# Get all .cs files in the target directory
$csFiles = Get-ChildItem -Path $TargetDirectory -Filter "*.cs" -Recurse | Where-Object { $_.Name -notlike "*Handler.cs" -and $_.Name -notlike "*Command.cs" -and $_.Name -notlike "*Query.cs" }

$replacements = @{
    # Basic assertions
    '\.Should\(\)\.Be\(' = '.ShouldBe('
    '\.Should\(\)\.NotBe\(' = '.ShouldNotBe('
    '\.Should\(\)\.BeNull\(\)' = '.ShouldBeNull()'
    '\.Should\(\)\.NotBeNull\(\)' = '.ShouldNotBeNull()'
    '\.Should\(\)\.BeTrue\(\)' = '.ShouldBeTrue()'
    '\.Should\(\)\.BeFalse\(\)' = '.ShouldBeFalse()'
    '\.Should\(\)\.BeOfType<([^>]+)>\(\)' = '.ShouldBeOfType<$1>()'
    '\.Should\(\)\.BeAssignableTo<([^>]+)>\(\)' = '.ShouldBeAssignableTo<$1>()'
    
    # Comparison assertions
    '\.Should\(\)\.BeGreaterThan\(' = '.ShouldBeGreaterThan('
    '\.Should\(\)\.BeGreaterThanOrEqualTo\(' = '.ShouldBeGreaterThanOrEqualTo('
    '\.Should\(\)\.BeLessThan\(' = '.ShouldBeLessThan('
    '\.Should\(\)\.BeLessThanOrEqualTo\(' = '.ShouldBeLessThanOrEqualTo('
    
    # Collection assertions
    '\.Should\(\)\.HaveCount\(' = '.Count.ShouldBe('
    '\.Should\(\)\.Contain\(' = '.ShouldContain('
    '\.Should\(\)\.NotContain\(' = '.ShouldNotContain('
    '\.Should\(\)\.OnlyContain\(' = '.ShouldAllBe('
    '\.Should\(\)\.ContainKey\(' = '.ShouldContainKey('
    '\.Should\(\)\.ContainValue\(' = '.ShouldContainValue('
    
    # String assertions
    '\.Should\(\)\.StartWith\(' = '.ShouldStartWith('
    '\.Should\(\)\.EndWith\(' = '.ShouldEndWith('
    '\.Should\(\)\.Match\(' = '.ShouldMatch('
    
    # Exception assertions - these need manual review
    '\.Should\(\)\.Throw<([^>]+)>\(\)' = '.ShouldThrow<$1>()'
    '\.Should\(\)\.NotThrow\(\)' = '.ShouldNotThrow()'
}

foreach ($file in $csFiles) {
    Write-Host "Processing: $($file.FullName)" -ForegroundColor Yellow
    
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    
    foreach ($pattern in $replacements.Keys) {
        $replacement = $replacements[$pattern]
        $content = $content -replace $pattern, $replacement
    }
    
    # Special handling for some complex patterns
    $content = $content -replace '\.Should\(\)\.ContainKey\(([^)]+)\)\.And\.Subject\[([^]]+)\]\.Should\(\)\.Be\(([^)]+)\)', '.ShouldContainKeyAndValue($1, $3)'
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        Write-Host "  Updated: $($file.Name)" -ForegroundColor Cyan
    } else {
        Write-Host "  No changes: $($file.Name)" -ForegroundColor Gray
    }
}

Write-Host "Conversion completed!" -ForegroundColor Green
