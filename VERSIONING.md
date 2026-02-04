# Version Consistency in TQVaultAE Suite

## Overview

The TQVaultAE suite consists of 3 executable applications and 7 DLLs (1 .NET Framework + 6 .NET Standard 2.0) that maintain **version consistency**:

| Type | Output | Project | Target Framework | Assembly Name |
|-------|--------|---------|-----------------|---------------|
| EXE | TQVaultAE.exe | TQVaultAE.GUI | .NET Framework 4.8 | TQVaultAE |
| EXE | TQ.SaveFilesExplorer.exe | TQSaveFilesExplorer | .NET Framework 4.8 | Titan Quest Save Files Explorer |
| EXE | ARZExplorer.exe | ARZExplorer | .NET Framework 4.8 | ARZExplorer |
| DLL | TQVaultAE.Services.Win32.dll | TQVaultAE.Services.Win32 | .NET Framework 4.8 | TQVaultAE.Services.Win32 |
| DLL | TQVaultAE.Domain.dll | TQVaultAE.Domain | netstandard2.0 | TQVaultAE.Domain |
| DLL | TQVaultAE.Services.dll | TQVaultAE.Services | netstandard2.0 | TQVaultAE.Services |
| DLL | TQVaultAE.Presentation.dll | TQVaultAE.Presentation | netstandard2.0 | TQVaultAE.Presentation |
| DLL | TQVaultAE.Data.dll | TQVaultAE.Data | netstandard2.0 | TQVaultAE.Data |
| DLL | TQVaultAE.Config.dll | TQVaultAE.Config | netstandard2.0 | TQVaultAE.Config |
| DLL | TQVaultAE.Logs.dll | TQVaultAE.Logs | netstandard2.0 | TQVaultAE.Logs |

## Current Version Status

✅ **ALL VERSIONS SYNCHRONIZED** ✅

All projects currently use version **4.4.0.0**:

| Project Type | Current Version | Files |
|--------------|----------------|-------|
| .NET Framework (AssemblyInfo.cs) | **4.4.0.0** | 4 files |
| .NET Standard (.csproj) | **4.4.0.0** | 6 files |

### Current State by File

**AssemblyInfo.cs files (.NET Framework - 4.4.0.0):**
```
src/TQVaultAE.GUI/Properties/AssemblyInfo.cs: 4.4.0.0
src/TQSaveFilesExplorer/Properties/AssemblyInfo.cs: 4.4.0.0
src/ARZExplorer/Properties/AssemblyInfo.cs: 4.4.0.0
src/TQVaultAE.Services.Win32/Properties/AssemblyInfo.cs: 4.4.0.0
```

**.csproj files (.NET Standard 2.0 - 4.4.0.0):**
```
src/TQVaultAE.Domain/TQVaultAE.Domain.csproj: 4.4.0.0
src/TQVaultAE.Services/TQVaultAE.Services.csproj: 4.4.0.0
src/TQVaultAE.Presentation/TQVaultAE.Presentation.csproj: 4.4.0.0
src/TQVaultAE.Data/TQVaultAE.Data.csproj: 4.4.0.0
src/TQVaultAE.Config/TQVaultAE.Config.csproj: 4.4.0.0
src/TQVaultAE.Logs/TQVaultAE.Logs.csproj: 4.4.0.0
```

**version-info.json:**
```json
{
  "Major": 4,
  "Minor": 4,
  "Build": 0,
  "Revision": 0
}
```

## Version Format

All executables and DLLs use the same **4-part version number**:

```
Major.Minor.Build.Revision
```

Example: `4.4.0.0`
- **Major** (4): Major version, breaking changes
- **Minor** (4): Minor version, new features
- **Build** (0): Build number (increments on manual builds, reset to 0 on releases)
- **Revision** (0): Reserved (currently 0)

## Why Consistent Versioning?

### Benefits
1. **Unified Release Management** - All tools released together
2. **Clear Version Tracking** - Single version for entire suite
3. **Easier Support** - Know exact version across all tools
4. **User Clarity** - No confusion about which tool uses which version
5. **Dependency Alignment** - Services library version matches executables

### Use Cases

**User Scenario:**
```
"I'm using TQVaultAE v4.4.0 and TQ.SaveFilesExplorer v3.2.1"
→ This is confusing and potentially incompatible
→ With consistent versioning: "I'm using TQVaultAE Suite v4.4.0"
```

**Developer Scenario:**
```
Bug report: "Save file fails in v4.4.0"
→ Developer knows exactly which codebase version
→ All tools tested with same version
→ Reproducible across entire suite
```

## Implementation

### Version Source

Single source of truth: `version-info.json`

```json
{
  "Major": 4,
  "Minor": 4,
  "Build": 0,
  "Revision": 0
}
```

### Update Script

`Update-Version.ps1` script ensures consistency:

1. Reads version from `version-info.json`
2. Updates ALL AssemblyInfo.cs files with SAME version
3. Updates ALL .NET SDK .csproj files with SAME version
4. Saves updated version back to `version-info.json`

### Version Increment Rules

| Event | Version Change | Example |
|-------|----------------|----------|
| PR merged to `main` (release) | Minor +1, Build=0 | `4.4.0.0` → `4.5.0.0` |
| `feature/*` push or PR | None | Uses current version |

### Files Updated

Each update modifies ALL these files:

```
version-info.json
src/TQVaultAE.GUI/Properties/AssemblyInfo.cs
src/TQSaveFilesExplorer/Properties/AssemblyInfo.cs
src/ARZExplorer/Properties/AssemblyInfo.cs
src/TQVaultAE.Services.Win32/Properties/AssemblyInfo.cs
src/TQVaultAE.Domain/TQVaultAE.Domain.csproj
src/TQVaultAE.Services/TQVaultAE.Services.csproj
src/TQVaultAE.Presentation/TQVaultAE.Presentation.csproj
src/TQVaultAE.Data/TQVaultAE.Data.csproj
src/TQVaultAE.Config/TQVaultAE.Config.csproj
src/TQVaultAE.Logs/TQVaultAE.Logs.csproj
```

All receive **identical version** on every update.

## Verification

### Before Build

```bash
# Check AssemblyVersion in all .NET Framework projects
grep "AssemblyVersion" src/*/Properties/AssemblyInfo.cs

# Check Version in all .NET SDK-style projects
grep "<Version>" src/*/*.csproj
```

Expected output (all lines show same version):
```
# AssemblyInfo.cs files (.NET Framework)
src/TQVaultAE.GUI/Properties/AssemblyInfo.cs:[assembly: AssemblyVersion("4.4.0.0")]
src/TQSaveFilesExplorer/Properties/AssemblyInfo.cs:[assembly: AssemblyVersion("4.4.0.0")]
src/ARZExplorer/Properties/AssemblyInfo.cs:[assembly: AssemblyVersion("4.4.0.0")]
src/TQVaultAE.Services.Win32/Properties/AssemblyInfo.cs:[assembly: AssemblyVersion("4.4.0.0")]

# csproj files (.NET Standard 2.0)
src/TQVaultAE.Domain/TQVaultAE.Domain.csproj:    <Version>4.4.0.0</Version>
src/TQVaultAE.Services/TQVaultAE.Services.csproj:    <Version>4.4.0.0</Version>
src/TQVaultAE.Presentation/TQVaultAE.Presentation.csproj:    <Version>4.4.0.0</Version>
src/TQVaultAE.Data/TQVaultAE.Data.csproj:    <Version>4.4.0.0</Version>
src/TQVaultAE.Config/TQVaultAE.Config.csproj:    <Version>4.4.0.0</Version>
src/TQVaultAE.Logs/TQVaultAE.Logs.csproj:    <Version>4.4.0.0</Version>
```

### After Build

```powershell
# Check compiled executables
(Get-Item "src\TQVaultAE.GUI\bin\AnyCPU\Release\TQVaultAE.exe").VersionInfo.FileVersion
(Get-Item "src\TQSaveFilesExplorer\bin\Release\TQ.SaveFilesExplorer.exe").VersionInfo.FileVersion
(Get-Item "src\ARZExplorer\bin\Release\ARZExplorer.exe").VersionInfo.FileVersion

# Check compiled DLLs
(Get-Item "src\TQVaultAE.Domain\bin\Release\netstandard2.0\TQVaultAE.Domain.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Services\bin\Release\netstandard2.0\TQVaultAE.Services.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Presentation\bin\Release\netstandard2.0\TQVaultAE.Presentation.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Data\bin\Release\netstandard2.0\TQVaultAE.Data.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Config\bin\Release\netstandard2.0\TQVaultAE.Config.dll").VersionInfo.FileVersion
(Get-Item "src\TQVaultAE.Logs\bin\Release\netstandard2.0\TQVaultAE.Logs.dll").VersionInfo.FileVersion
```

All should return: `4.4.0.0` (or current synchronized version)

### From Executable Properties

1. Right-click executable → Properties → Details tab
2. Check "File version" field
3. All executables and DLLs should show same version

## Version History

| Version | Date | Release Notes |
|---------|-------|---------------|
| 4.4.0.0 | Current | All files synchronized |
| 4.5.0.0 | Future | **Next official release** to main |
| 4.6.0.0 | Future | **Official release** to main |

- **Releases to main**: Minor version increments, Build=0
- **Feature/PR builds**: No version change

## Troubleshooting

### Issue: Versions Don't Match

**Symptom:**
```
TQVaultAE.exe: 4.4.0.0
TQ.SaveFilesExplorer.exe: 4.3.0.0
```

**Solution:**
1. Run the version update script to synchronize all files:
   ```bash
   pwsh -File .github/scripts/Update-Version.ps1 -VersionType "Release"
   ```
2. Or manually update `version-info.json` to desired version (e.g., 4.5.0.0)
3. Run the update script to apply to all files
4. Verify all files now show same version:
   ```bash
   grep "AssemblyVersion" src/*/Properties/AssemblyInfo.cs
   grep "<Version>" src/*/*.csproj
   ```
5. Commit the changes

### Issue: Wildcard Version in AssemblyInfo

**Symptom:**
```
[assembly: AssemblyVersion("4.4.*")]
```

**Solution:**
The update script replaces wildcards with exact version. After running Update-Version.ps1, files should have:

AssemblyInfo.cs files (.NET Framework):
```
[assembly: AssemblyVersion("4.4.0.0")]
```

csproj files (.NET Standard 2.0):
```
<Version>4.4.0.0</Version>
```

### Issue: Version Not Incrementing on Release

**Symptom:**
Version doesn't increment on merge to main.

**Solution:**
1. Check workflow logs in GitHub Actions
2. Verify Update-Version.ps1 ran successfully
3. Check that version was committed back to main branch
4. Ensure workflow has write permissions (secrets.GITHUB_TOKEN)

## Best Practices

1. **Never manually edit AssemblyInfo.cs** - Use Update-Version.ps1
2. **Always verify consistency** - Check all executables before release
3. **Use version-info.json** - Single source of truth
4. **Test all executables** - Ensure they work at same version
5. **Document version changes** - Keep release notes for all tools
6. **Maintain synchronization** - All 10 files must have identical versions

## Next Release Preparation

When ready to release version 4.5.0.0:

```bash
# Option 1: Let CI/CD handle it automatically on merge to main
# The workflow will automatically increment Minor version

# Option 2: Manual update before release
# Edit version-info.json: "Minor": 5, "Build": 0
pwsh -File .github/scripts/Update-Version.ps1 -VersionType "Release"

# Verify synchronization
grep "AssemblyVersion" src/*/Properties/AssemblyInfo.cs
grep "<Version>" src/*/*.csproj

# Commit and push
git add -A
git commit -m "chore: bump version to 4.5.0.0"
git push
```

## Related Documentation

- [GitHub Actions Build and Test Guide](GITHUB_ACTIONS_BUILD_TEST.md)
- [Agent Guidelines](AGENTS.md)
- [Contributing Guide](CONTRIBUTING.md)
