param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Sync")]
    [string]$VersionType,

    [Parameter(Mandatory=$false)]
    [string]$SolutionRoot = $PSScriptRoot + "\..\..",

    [Parameter(Mandatory=$false)]
    [string]$VersionFile = "$SolutionRoot\version-info.json"
)

Write-Host "Version Management Script"
Write-Host "======================="
Write-Host "Version Type: $VersionType"
Write-Host "Solution Root: $SolutionRoot"
Write-Host "Version File: $VersionFile"
Write-Host ""

$versionInfo = @{}

if (Test-Path $VersionFile) {
    $jsonContent = Get-Content $VersionFile -Raw | ConvertFrom-Json
    $versionInfo = @{
        Major = $jsonContent.Major
        Minor = $jsonContent.Minor
        Build = $jsonContent.Build
        Revision = $jsonContent.Revision
    }
    Write-Host "Loaded version from file: $($versionInfo.Major).$($versionInfo.Minor).$($versionInfo.Build).$($versionInfo.Revision)"
} else {
    Write-Host "Error: version-info.json not found!"
    exit 1
}

$newVersion = "$($versionInfo.Major).$($versionInfo.Minor).$($versionInfo.Build).$($versionInfo.Revision)"
Write-Host "Syncing version: $newVersion"
Write-Host ""

$versionJson = @{
    Major = $versionInfo.Major
    Minor = $versionInfo.Minor
    Build = $versionInfo.Build
    Revision = $versionInfo.Revision
} | ConvertTo-Json

Set-Content -Path $VersionFile -Value $versionJson -Encoding UTF8
Write-Host "Saved version info to: $VersionFile"
Write-Host ""

$assemblyInfoFiles = @(
    "$SolutionRoot\src\TQVaultAE.GUI\Properties\AssemblyInfo.cs",
    "$SolutionRoot\src\TQSaveFilesExplorer\Properties\AssemblyInfo.cs",
    "$SolutionRoot\src\ARZExplorer\Properties\AssemblyInfo.cs",
    "$SolutionRoot\src\TQVaultAE.Services.Win32\Properties\AssemblyInfo.cs"
)

foreach ($file in $assemblyInfoFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw -Encoding UTF8
        
        $originalContent = $content
        $content = $content -replace '\[assembly:\s*AssemblyVersion\(".*?"\)\]', "[assembly: AssemblyVersion(`"$newVersion`")]"
        $content = $content -replace '\[assembly:\s*AssemblyFileVersion\(".*?"\)\]', "[assembly: AssemblyFileVersion(`"$newVersion`")]"
        
        if ($content -ne $originalContent) {
            Set-Content -Path $file -Value $content -Encoding UTF8 -NoNewline
            Write-Host "Updated: $file"
        } else {
            Write-Host "Skipped (no changes): $file"
        }
    } else {
        Write-Host "File not found: $file"
    }
}

$csprojFiles = @(
    "$SolutionRoot\src\TQVaultAE.Domain\TQVaultAE.Domain.csproj",
    "$SolutionRoot\src\TQVaultAE.Services\TQVaultAE.Services.csproj",
    "$SolutionRoot\src\TQVaultAE.Presentation\TQVaultAE.Presentation.csproj",
    "$SolutionRoot\src\TQVaultAE.Data\TQVaultAE.Data.csproj",
    "$SolutionRoot\src\TQVaultAE.Config\TQVaultAE.Config.csproj"
    "$SolutionRoot\src\TQVaultAE.Logs\TQVaultAE.Logs.csproj"
)

foreach ($file in $csprojFiles) {
    if (Test-Path $file) {
        $content = Get-Content $file -Raw -Encoding UTF8
        
        $originalContent = $content
        
        if ($content -match '<Version>.*</Version>') {
            $content = $content -replace '<Version>.*</Version>', "<Version>$newVersion</Version>"
            
            if ($content -ne $originalContent) {
                Set-Content -Path $file -Value $content -Encoding UTF8 -NoNewline
                Write-Host "Updated: $file"
            } else {
                Write-Host "Skipped (no changes): $file"
            }
        } else {
            Write-Host "Skipped (no <Version> tag): $file"
        }
    } else {
        Write-Host "File not found: $file"
    }
}

Write-Host ""
Write-Host "Version sync complete: $newVersion"
Write-Host ""
Write-Host "Files Updated:"
Write-Host "- 4 AssemblyInfo.cs files (EXE projects)"
Write-Host "- 6 csproj files (NET Standard DLL projects)"
Write-Host "- Total: 10 project files synchronized with version-info.json"