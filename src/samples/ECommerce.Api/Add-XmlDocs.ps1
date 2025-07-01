# PowerShell script to add XML documentation comments to C# files
# This script will add basic XML documentation to public classes and members

param(
    [string]$ProjectPath = "."
)

function Add-XmlDocumentation {
    param(
        [string]$FilePath
    )
    
    Write-Host "Processing $FilePath"
    
    # Read the file content
    $content = Get-Content $FilePath -Raw
    
    # Add class documentation
    $content = $content -replace '(?m)^(\s*)public class (\w+)', '$1/// <summary>${1}/// Represents $2 in the application.${1}/// </summary>${1}public class $2'
    $content = $content -replace '(?m)^(\s*)public enum (\w+)', '$1/// <summary>${1}/// Enumeration for $2.${1}/// </summary>${1}public enum $2'
    $content = $content -replace '(?m)^(\s*)public interface (\w+)', '$1/// <summary>${1}/// Interface for $2.${1}/// </summary>${1}public interface $2'
    
    # Add property documentation
    $content = $content -replace '(?m)^(\s*)public (\w+(?:\<[^>]*\>)?) (\w+) \{ get; set; \}', '$1/// <summary>${1}/// Gets or sets the $3.${1}/// </summary>${1}public $2 $3 { get; set; }'
    
    # Add method documentation for simple methods
    $content = $content -replace '(?m)^(\s*)public async Task\<(\w+)\> (\w+)\(([^)]*)\)', '$1/// <summary>${1}/// $3 method.${1}/// </summary>${1}public async Task<$2> $3($4)'
    $content = $content -replace '(?m)^(\s*)public Task\<(\w+)\> (\w+)\(([^)]*)\)', '$1/// <summary>${1}/// $3 method.${1}/// </summary>${1}public Task<$2> $3($4)'
    $content = $content -replace '(?m)^(\s*)public (\w+) (\w+)\(([^)]*)\)', '$1/// <summary>${1}/// $3 method.${1}/// </summary>${1}public $2 $3($4)'
    
    # Write the content back
    Set-Content -Path $FilePath -Value $content -NoNewline
}

# Find all C# files in Application folder
$files = Get-ChildItem -Path "$ProjectPath\Application" -Filter "*.cs" -Recurse

foreach ($file in $files) {
    Add-XmlDocumentation -FilePath $file.FullName
}

Write-Host "XML documentation added to $($files.Count) files"
