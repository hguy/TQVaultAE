# GitHub Actions Integration Guide: Build and Test for TQVaultAE Projects

This document provides comprehensive steps for integrating GitHub Actions to build and test the TQVaultAE .NET Framework projects.

## Build Strategy

| Branch/Trigger | Configurations | Retention | Purpose |
|----------------|---------------|-----------|---------|
| `main` push | Release only | Permanent (GitHub Releases) | Production release |
| Pull Request | Release only | 3 days (artifacts) | PR verification |
| `feature/*` push | Release only | 3 days (artifacts) | Feature development |

### Key Principles
- **Main branch**: Release builds only, published as GitHub Releases (permanent)
- **Feature branches**: Release builds only for PR verification
- **Release builds**: For production distribution on main branch
- **No Debug builds in CI**: Build locally for development/debugging

## Prerequisites

- Repository: https://github.com/EtienneLamoureux/TQVaultAE
- GitHub repository with Actions enabled
- Windows runner support (required for .NET Framework)
- ImageMagick.Q8: Required by Magick.NET package for DDS to PNG conversion (installed via winget)
- **Personal Access Token (PAT)**: Required to bypass branch protection rules when pushing version changes (see setup below)

### Setting Up the Version Bump Token (PAT)

The workflow requires a Personal Access Token to push commits and create tags on protected branches. The default `GITHUB_TOKEN` cannot bypass branch protection rules.

**Steps to create the PAT:**

1. Go to your GitHub account → **Settings** → **Developer settings** → **Personal access tokens** → **Tokens (classic)**
2. Click **Generate new token (classic)**
3. Give it a name like "TQVaultAE Version Bump"
4. Select scopes:
   - ✅ `repo` - Full control of private repositories
   - ✅ `workflow` - Update GitHub Action workflows
5. Click **Generate token**
6. **Copy the token immediately** (you won't see it again)

**Add the token to your repository:**

1. Go to your repository on GitHub → **Settings** → **Secrets and variables** → **Actions**
2. Click **New repository secret**
3. Name: `VERSION_BUMP_TOKEN`
4. Value: Paste your PAT from step 6
5. Click **Add secret**

**Configure branch protection to allow the PAT:**

1. Go to **Settings** → **Branches**
2. Click **Edit** on your protection rule for `main`/`master`
3. Under "Allow specified actors to bypass required pull requests", add the GitHub user account associated with the PAT
4. Alternatively, enable **Allow force pushes** and **Allow deletions** for admin users
5. Click **Save changes**

**Note**: If using an organization account, the PAT owner must have appropriate permissions in the organization.

## Projects to Build

| Project | Target Framework | Output Type | Path |
|---------|-----------------|-------------|------|
| TQVaultAE.GUI | .NET Framework 4.8 | WinExe | src/TQVaultAE.GUI/TQVaultAE.GUI.csproj |
| TQ.SaveFilesExplorer | .NET Framework 4.8 | WinExe | src/TQSaveFilesExplorer/TQ.SaveFilesExplorer.csproj |
| ARZExplorer | .NET Framework 4.8 | WinExe | src/ARZExplorer/ArzExplorer.csproj |

## Test Projects

| Project | Path |
|---------|------|
| TQVaultAE.Tests | src/TQVaultAE.Tests/TQVaultAE.Tests.csproj |

## Why This Build Strategy?

### Main Branch (Production Releases)
- **Release builds only** - optimized for production
- **GitHub Releases** - permanent storage for version history
- **Published artifacts** available for users indefinitely
- **Version increment** - Minor version bumps on release

### Pull Requests (Verification)
- **Release builds only** - verify code compiles and tests pass
- **3-day retention** - sufficient for PR review and quick feedback
- **Faster feedback loop** for contributors
- **No version changes** - uses current version

### Feature Branches
- **Same as PR** - Release builds, 3-day retention
- **Local debugging** - developers build Debug configuration locally

## Version Management

Version management is **manual** via `version-info.json`. When you edit this file and push to `main`/`master`, CI automatically syncs the version to all project files and creates a release.

For detailed information about version consistency implementation, see [VERSIONING.md](VERSIONING.md).

### How It Works

1. **Edit version-info.json**: Update the version numbers manually
2. **Push to main/master**: Commit and push the change
3. **Auto-sync**: CI detects the change and syncs to all `AssemblyInfo.cs` and `.csproj` files
4. **Auto-release**: CI creates a git tag and GitHub Release

### Why This Approach?

**No infinite loops**: The sync only triggers when `version-info.json` changes. The subsequent commit (with synced files) won't trigger another sync because `version-info.json` wasn't modified in that push.

**Manual control**: You decide when to release by editing the version file, rather than CI auto-incrementing on every merge.

### Version Format

```
Major.Minor.Build.Revision
```

Example: `4.4.1.0`
- **Major** (4): Breaking changes, major features
- **Minor** (4): New features, backward compatible  
- **Build** (1): Build number (manually managed or for hotfixes)
- **Revision** (0): Reserved (currently 0)

### To Release a New Version

1. Edit `version-info.json` with your new version:
   ```json
   {
     "Major": 4,
     "Minor": 5,
     "Build": 0,
     "Revision": 0
   }
   ```

2. Commit and push to `main`/`master`:
   ```bash
   git add version-info.json
   git commit -m "chore: bump version to 4.5.0"
   git push origin main
   ```

3. CI will automatically:
   - Sync the version to all 11 project files
   - Build and test
   - Create a git tag (e.g., `4.5.0`)
   - Create a GitHub Release with artifacts

### Version Consistency

**All 3 executables and 7 DLLs share the same version:**

### Executable Files (.NET Framework)

- `TQVaultAE.exe` (from TQVaultAE.GUI - .NET Framework 4.8)
- `TQ.SaveFilesExplorer.exe` (from TQSaveFilesExplorer - .NET Framework 4.8)
- `ARZExplorer.exe` (from ARZExplorer - .NET Framework 4.8)

### .NET Framework DLL

- `TQVaultAE.Services.Win32.dll` (from TQVaultAE.Services.Win32 - .NET Framework 4.8)

### .NET Standard 2.0 DLLs

- `TQVaultAE.Domain.dll` (netstandard2.0)
- `TQVaultAE.Services.dll` (netstandard2.0)
- `TQVaultAE.Presentation.dll` (netstandard2.0)
- `TQVaultAE.Data.dll` (netstandard2.0)
- `TQVaultAE.Config.dll` (netstandard2.0)
- `TQVaultAE.Logs.dll` (netstandard2.0)

This ensures:
- **Unified versioning** across the entire TQVaultAE suite (3 EXEs + 7 DLLs)
- **Consistent releases** - all tools and libraries update together
- **Clear tracking** - same version across all executables and DLLs
- **Single version source** - `version-info.json` controls all projects

### Files Modified

- `version-info.json` - Version storage
- `src/TQVaultAE.GUI/Properties/AssemblyInfo.cs`
- `src/TQSaveFilesExplorer/Properties/AssemblyInfo.cs`
- `src/ARZExplorer/Properties/AssemblyInfo.cs`
- `src/TQVaultAE.Services.Win32/Properties/AssemblyInfo.cs`
- `src/TQVaultAE.Domain/TQVaultAE.Domain.csproj`
- `src/TQVaultAE.Services/TQVaultAE.Services.csproj`
- `src/TQVaultAE.Presentation/TQVaultAE.Presentation.csproj`
- `src/TQVaultAE.Data/TQVaultAE.Data.csproj`
- `src/TQVaultAE.Config/TQVaultAE.Config.csproj`
- `src/TQVaultAE.Logs/TQVaultAE.Logs.csproj`

**Benefits:**
- **Permanent version history**: GitHub Releases maintain all production versions
- **User-ready**: Production releases are optimized and permanent
- **Automatic versioning**: CI/CD handles version increments on release
- **Cost-effective**: Short retention for temporary PR/feature builds

### Step 1: Create GitHub Actions Workflow Directory

Create the following directory structure in your repository:

```
.github/
  workflows/
    build-and-test.yml
  scripts/
    Update-Version.ps1
```

**Workflow Strategy:**
- Triggers on pushes to `main` branch, pull requests, and feature branches
- **Main branch**: Builds Release only, creates permanent GitHub Release, auto-increment Minor version on merge
- **Pull requests/Feature branches**: Builds Release only, 3-day artifact retention, no version increment

```
.github/
  workflows/
    build-and-test.yml
```

### Step 2: Create the Workflow File

Create `.github/workflows/build-and-test.yml` with the following content:

**Important**: The workflow includes an ImageMagick installation step using `winget install ImageMagick.Q8`. This is required by the Magick.NET-Q8-AnyCPU package for DDS to PNG conversion. The installation must occur **before** the build step.

```yaml
name: Build and Test

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:

env:
  DOTNET_VERSION: '8.0.x'
  SOLUTION_FILE: 'TQVaultAE.sln'

jobs:
  build:
    runs-on: windows-latest
    
    steps:
    - name: Checkout repository
      uses: actions/checkout@v4
      with:
        fetch-depth: 0
        token: ${{ secrets.VERSION_BUMP_TOKEN }}
    
    - name: Check if version-info.json changed
      id: version-check
      run: |
        $changed = git diff --name-only HEAD~1 HEAD | Select-String -Pattern "version-info.json" -Quiet
        if ($changed) {
          Write-Output "changed=true" >> $env:GITHUB_OUTPUT
        } else {
          Write-Output "changed=false" >> $env:GITHUB_OUTPUT
        }
      shell: pwsh
    
    - name: Setup MSBuild path
      uses: microsoft/setup-msbuild@v2
    
    - name: Setup NuGet
      uses: NuGet/setup-nuget@v2
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Install ImageMagick
      run: winget install ImageMagick.Q8 --accept-source-agreements --accept-package-agreements
    
    - name: Sync Version (main release)
      if: github.ref == 'refs/heads/main' && github.event_name == 'push' && steps.version-check.outputs.changed == 'true'
      run: pwsh -File .github/scripts/Update-Version.ps1 -VersionType "Sync" || echo "Version sync completed"
    
    - name: Commit version changes (main)
      if: github.ref == 'refs/heads/main' && github.event_name == 'push' && steps.version-check.outputs.changed == 'true'
      run: |
        git config user.name "github-actions[bot]"
        git config user.email "github-actions[bot]@users.noreply.github.com"
        git add version-info.json src/TQVaultAE.GUI/Properties/AssemblyInfo.cs src/TQSaveFilesExplorer/Properties/AssemblyInfo.cs src/ARZExplorer/Properties/AssemblyInfo.cs src/TQVaultAE.Services.Win32/Properties/AssemblyInfo.cs src/TQVaultAE.Domain/TQVaultAE.Domain.csproj src/TQVaultAE.Services/TQVaultAE.Services.csproj src/TQVaultAE.Presentation/TQVaultAE.Presentation.csproj src/TQVaultAE.Data/TQVaultAE.Data.csproj src/TQVaultAE.Config/TQVaultAE.Config.csproj src/TQVaultAE.Logs/TQVaultAE.Logs.csproj
        git commit -m "chore: sync version to all project files" || echo "No changes to commit"
        git push || echo "Nothing to push"
    
    - name: Create version tag (main)
      if: github.ref == 'refs/heads/main' && github.event_name == 'push' && steps.version-check.outputs.changed == 'true'
      run: |
        $versionInfo = Get-Content version-info.json | ConvertFrom-Json
        $tagName = "$($versionInfo.Major).$($versionInfo.Minor).$($versionInfo.Build)"
        git tag $tagName || echo "Tag already exists"
        git push origin $tagName || echo "Tag push skipped"
    
    - name: Restore NuGet packages
      run: nuget restore ${{ env.SOLUTION_FILE }}
    
    - name: Build Release
      run: msbuild ${{ env.SOLUTION_FILE }} /p:Configuration=Release /p:Platform="Any CPU" /m /v:minimal
    
    - name: Upload TQVaultAE.GUI artifacts (Release)
      uses: actions/upload-artifact@v4
      with:
        name: TQVaultAE-GUI-Release
        path: src/TQVaultAE.GUI/bin/AnyCPU/Release/
        retention-days: 3
    
    - name: Upload TQ.SaveFilesExplorer artifacts (Release)
      uses: actions/upload-artifact@v4
      with:
        name: TQ-SaveFilesExplorer-Release
        path: src/TQSaveFilesExplorer/bin/Release/
        retention-days: 3
    
    - name: Upload ARZExplorer artifacts (Release)
      uses: actions/upload-artifact@v4
      with:
        name: ARZExplorer-Release
        path: src/ARZExplorer/bin/Release/
        retention-days: 3
    
    - name: Create GitHub Release (main branch)
      if: github.ref == 'refs/heads/main' && github.event_name == 'push' && steps.version-check.outputs.changed == 'true'
      uses: softprops/action-gh-release@v1
      with:
        files: |
          src/TQVaultAE.GUI/bin/AnyCPU/Release/**/*
          src/TQSaveFilesExplorer/bin/Release/**/*
          src/ARZExplorer/bin/Release/**/*
        draft: false
        prerelease: false
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}

  test:
    runs-on: windows-latest
    needs: build
    
    steps:
    - name: Checkout repository
      uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore src/TQVaultAE.Tests/TQVaultAE.Tests.csproj
    
    - name: Build test project
      run: dotnet build src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --configuration Release
    
    - name: Run tests
      run: dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --configuration Release --verbosity normal --no-build --logger "trx;LogFileName=test-results.trx"
    
    - name: Upload test results
      uses: actions/upload-artifact@v4
      if: always()
      with:
        name: test-results
        path: '**/TestResults/*.trx'
        retention-days: 3
    
    - name: Publish test results
      uses: EnricoMi/publish-unit-test-result-action@v2
      if: always()
      with:
        files: '**/TestResults/*.trx'
```

### Step 3: Workflow Configuration Options

#### Trigger Options

The workflow can be triggered by:
- **Push events**: Runs when code is pushed to `main` branch
- **Pull requests**: Runs when PRs are created/updated against `main`
- **Workflow dispatch**: Manual trigger from GitHub Actions tab
- **Scheduled runs**: Add `schedule` trigger for periodic builds

Example of scheduled builds:

```yaml
on:
  schedule:
    - cron: '0 0 * * 0'  # Weekly on Sunday at midnight
  push:
    branches: [ main ]
```

#### Runner Options

- `windows-latest`: Current Windows runner (recommended for .NET Framework)
- `windows-2022`: Specific Windows Server 2022 runner
- `windows-2019`: Windows Server 2019 runner

#### MSBuild Configuration Options

| Option | Description | Example |
|--------|-------------|---------|
| `/p:Configuration` | Build configuration (Release for distribution) | `/p:Configuration=Release` |
| `/p:Platform` | Target platform | `/p:Platform="Any CPU"` or `/p:Platform=x86` |
| `/m` | Enable parallel builds | `/m:4` for 4 parallel processes |
| `/v` | Verbosity level | `/v:minimal`, `/v:normal`, `/v:detailed` |

**Note**: For local Debug builds, use `/p:Configuration=Debug` |

#### Test Options

```yaml
# Run all tests (Release configuration for production CI/CD)
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --configuration Release

# Run specific test class
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --filter "FullyQualifiedName~GameFileServiceTests"

# Run specific test
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --filter "FullyQualifiedName~GameFileServiceTests.GetGamePath_ReturnsExpected"

# Run tests with coverage
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Note: Use Debug configuration locally for debugging with full symbols
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --configuration Debug
```

### Step 4: Advanced Workflow Features

#### Conditional Steps

```yaml
- name: Upload artifacts
  if: success()
  uses: actions/upload-artifact@v4
  with:
    name: build-output
    path: bin/Release/
```

#### Matrix Builds for Multiple Platforms

```yaml
strategy:
  matrix:
    platform: [AnyCPU, x86, x64]
    
runs-on: windows-latest

steps:
  - name: Build Release
    run: msbuild src/TQVaultAE.GUI/TQVaultAE.GUI.csproj /p:Configuration=Release /p:Platform=${{ matrix.platform }}
    if: github.ref == 'refs/heads/main'
```

**Note**: Matrix builds multiply execution time. Use only when testing multiple platforms is necessary.

#### Caching Dependencies

```yaml
- name: Cache NuGet packages
  uses: actions/cache@v4
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

### Step 5: Adding Status Badges

Add this markdown to your README.md to display workflow status:

```markdown
![Build Status](https://github.com/EtienneLamoureux/TQVaultAE/workflows/Build%20and%20Test/badge.svg)
```

### Step 6: Deployment Pipeline (Optional)

**Note**: The main workflow in Step 2 already creates GitHub Releases when code is pushed to `main` branch. This step is only needed if you want separate release jobs.

If you need additional release management:

```yaml
jobs:
  build:
    # ... build steps from Step 2 ...

  release:
    runs-on: windows-latest
    needs: build
    if: github.ref == 'refs/heads/main' && github.event_name == 'push'
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Download artifacts
      uses: actions/download-artifact@v4
      with:
        path: artifacts
    
    - name: Create GitHub Release with notes
      uses: softprops/action-gh-release@v1
      with:
        files: |
          artifacts/TQVaultAE-GUI-Release/**/*
          artifacts/TQ-SaveFilesExplorer-Release/**/*
          artifacts/ARZExplorer-Release/**/*
        body: |
          ## Release Notes
          - Add your release notes here
        draft: false
        prerelease: false
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

## Testing from a Fork

If you're testing these workflows from a fork (e.g., `https://github.com/hguy/TQVaultAE.git`), there are several important considerations:

### Fork-Specific Configuration

#### 1. Enable GitHub Actions on Your Fork
1. Go to your fork on GitHub
2. Navigate to **Settings** → **Actions** → **General**
3. Under "Actions permissions", select:
   - **Allow all actions and reusable workflows** (or at least "Allow local Actions only")
4. Check **Allow GitHub Actions to create and approve pull requests** (optional, for automated PRs)
5. Click **Save**

#### 2. Configure Workflow Permissions
The workflow already includes explicit permissions, but you should verify:

```yaml
permissions:
  contents: write    # Required for pushing tags and commits
  actions: read      # Required for reading workflow artifacts
  checks: write      # Required for publishing test results
```

#### 3. Important Fork Considerations

**Version Increment on Main Branch:**
- On your fork, when you push to `main`, the workflow will:
  1. Increment the version number
  2. Create a git tag (e.g., `4.5.0`)
  3. Push the changes back to your fork
  4. Create a GitHub Release on your fork

**This means:**
- Your fork will have its own version numbers (independent of the upstream repo)
- Tags will be created on your fork's `main` branch
- GitHub Releases will appear on your fork, not the upstream repo

#### 4. Testing Pull Requests from Your Fork

When you create a PR from your fork to the upstream repo:
- The workflow runs in the **upstream repository context**
- It uses the upstream repo's secrets and permissions
- The upstream maintainer controls whether Actions run on PRs from forks

**To test PR workflows on your fork:**
1. Create a feature branch on your fork: `git checkout -b feature/my-change`
2. Push to your fork: `git push origin feature/my-change`
3. Create a PR **within your fork** (fork's main ← fork's feature branch)
4. The workflow will run using your fork's settings

#### 5. Preventing Unintended Releases on Forks

If you want to test the workflow without creating actual releases on your fork, you can temporarily modify the conditions:

```yaml
# Original (creates releases on main)
if: github.ref == 'refs/heads/main' && github.event_name == 'push'

# Modified for testing (only run on upstream repo)
if: github.ref == 'refs/heads/main' && github.event_name == 'push' && github.repository == 'EtienneLamoureux/TQVaultAE'
```

Or add a check to skip releases on forks:

```yaml
- name: Skip releases on forks
  if: github.repository != 'EtienneLamoureux/TQVaultAE'
  run: echo "Skipping release creation on fork"
```

#### 6. Required Repository Settings

On your fork, ensure these settings are configured:

**Settings → Actions → General:**
- ✅ **Read and write permissions** (for workflows to push commits and tags)
- ✅ **Allow GitHub Actions to create and approve pull requests** (optional)

**Settings → Secrets and variables → Actions:**
- `VERSION_BUMP_TOKEN`: Your Personal Access Token (required to push to protected branches)

#### 7. Clean Up After Testing

If you've created test releases/tags on your fork:

```bash
# Delete local tags
git tag -d 4.5.0
git tag -d 4.6.0

# Delete remote tags on your fork
git push origin --delete 4.5.0
git push origin --delete 4.6.0

# Or delete all remote tags
# WARNING: Be careful with this!
git push origin --delete $(git tag -l)
```

**Delete GitHub Releases from your fork:**
1. Go to your fork on GitHub
2. Navigate to **Releases**
3. Click on each release → **Delete**

### Fork Testing Checklist

- [ ] GitHub Actions enabled on your fork
- [ ] Workflow permissions set to "Read and write"
- [ ] Tested on a feature branch first
- [ ] Understand that versions/tags will be created on your fork
- [ ] Know how to clean up test releases if needed

## Troubleshooting

### Common Issues

#### Issue: MSBuild not found
**Solution**: Ensure `microsoft/setup-msbuild@v2` action is used before MSBuild commands

#### Issue: NuGet package restore fails
**Solution**: Add `NuGet/setup-nuget@v2` action and use explicit restore before build

#### Issue: Test project doesn't build
**Solution**: Ensure .NET SDK version matches test project requirements (check .csproj)

#### Issue: Platform-specific build failures
**Solution**: Verify correct platform target in MSBuild command (AnyCPU vs x86 vs x64)

#### Issue: Artifact upload fails
**Solution**: Check that output paths exist and are correct relative to repository root

#### Issue: Magick.NET errors about ImageMagick not found
**Solution**: Ensure ImageMagick.Q8 is installed before build step using:
```yaml
- name: Install ImageMagick
  run: winget install ImageMagick.Q8 --accept-source-agreements --accept-package-agreements
```

#### Issue: Push permission denied on fork
**Solution**: Two scenarios:

**Scenario A - Branch protection on fork:**
If you see `GH006: Protected branch update failed`, your fork has branch protection enabled. The `GITHUB_TOKEN` cannot bypass these rules.
1. Create a Personal Access Token (see "Setting Up the Version Bump Token (PAT)" in Prerequisites)
2. Add it as `VERSION_BUMP_TOKEN` secret in your fork
3. The workflow uses this token automatically

**Scenario B - Read-only permissions:**
Forks run with read-only permissions by default. Fix this by:
1. Go to your fork on GitHub → **Settings** → **Actions** → **General**
2. Under "Workflow permissions", select **Read and write permissions**
3. Click **Save**

#### Issue: Git tag creation fails on fork
**Solution**: Three possible causes:
1. **Branch protection**: The `GITHUB_TOKEN` cannot bypass branch protection rules. Set up a `VERSION_BUMP_TOKEN` PAT (see Prerequisites section)
2. **Permissions**: Ensure workflow has `contents: write` permission (already included in current workflow)
3. **Settings**: Go to **Settings** → **Actions** → **General** and enable "Read and write permissions"

#### Issue: Version changes not appearing after workflow runs
**Solution**: On forks, the workflow pushes version changes back to your fork's `main` branch. Make sure to:
1. Pull changes after workflow runs: `git pull origin main`
2. Check your fork's commit history on GitHub
3. Verify the version in `version-info.json` was updated

#### Question: Where are Debug builds?
**Answer**: Debug builds are for local development. Build them locally using:
```bash
msbuild src/TQVaultAE.GUI/TQVaultAE.GUI.csproj /p:Configuration=Debug /p:Platform="Any CPU"
```

#### Question: How to get a copy of the build?
**Answer**: 
- **PR/Feature branches**: Download artifacts from GitHub Actions (3-day retention)
- **Main branch**: Download from GitHub Releases (permanent)

#### Question: Where are old PR builds?
**Answer**: PR artifacts have 3-day retention. After 3 days, they're automatically deleted. For permanent storage, merge to main branch which creates a GitHub Release.

## Testing Locally with Act

To test workflows locally:

```bash
# Install act (https://github.com/nektos/act)
# Linux/macOS
brew install act

# Test workflow
act push -j build -W .github/workflows/build-and-test.yml
```

## Best Practices

1. **Use Windows runners**: Required for .NET Framework projects
2. **Install ImageMagick**: Required by Magick.NET for image processing (install before build step)
3. **Release builds only**: Debug builds for local development only
4. **Appropriate retention**: 3 days for PRs/features, permanent for main via GitHub Releases
5. **Cache dependencies**: Speed up builds with NuGet caching
6. **Run tests on every push**: Catch issues early
7. **Separate build and test jobs**: Run in parallel when possible
8. **Upload test results**: Debug test failures with artifacts
9. **Use workflow dispatch**: Enable manual triggering
10. **Set artifact retention**: Balance storage costs with debugging needs
11. **Matrix builds**: Test multiple configurations efficiently
12. **Add status badges**: Display build status in README
13. **Document workflow**: Keep this guide updated with changes
14. **Automatic versioning**: Let CI/CD handle version increments on release
15. **Git tagging**: Automatic tags on main branch for easy version reference

## Workflow Summary

```
feature/* → PR → main
   ↓         ↓     ↓
  Verify   Verify  Release
 (3 days) (3 days) (permanent)
```

**Version Increment:** Only on merge to main (Minor +1, Build=0)

**Git Tagging:** Automatically creates a tag with format `Major.Minor.Build` (e.g., `4.5.0`) on each merge to main

**Retention:**
- PR/Feature builds: 3 days (temporary)
- Main releases: Permanent (GitHub Releases)

## Version Management Details

### Update-Version.ps1 Script

The PowerShell script handles automatic version updates:

**Parameters:**
- `-VersionType "Release"` - Increment minor number, reset build (for main branch)

**Usage Example:**
```bash
# Increment minor (release to main)
pwsh -File .github/scripts/Update-Version.ps1 -VersionType "Release"
```

### version-info.json

Tracks current version across the solution:

```json
{
  "Major": 4,
  "Minor": 4,
  "Build": 0,
  "Revision": 0
}
```

**Manual Updates:**
If you need to change the Major version manually:
1. Edit `version-info.json`
2. Update `Major` field
3. Commit changes
4. CI/CD will respect the new Major version

### Verifying Version Consistency

After version updates, verify all files have same version:

```bash
# Check AssemblyVersion in all .NET Framework projects
grep "AssemblyVersion" src/*/Properties/AssemblyInfo.cs
```

Expected output (all same version):
```
# AssemblyInfo.cs files (.NET Framework)
src/TQVaultAE.GUI/Properties/AssemblyInfo.cs:[assembly: AssemblyVersion("4.4.1.0")]
src/TQSaveFilesExplorer/Properties/AssemblyInfo.cs:[assembly: AssemblyVersion("4.4.1.0")]
src/ARZExplorer/Properties/AssemblyInfo.cs:[assembly: AssemblyVersion("4.4.1.0")]
src/TQVaultAE.Services.Win32/Properties/AssemblyInfo.cs:[assembly: AssemblyVersion("4.4.1.0")]
```

**Or check compiled executables:**
```bash
# Windows PowerShell
(Get-Item src/TQVaultAE.GUI/bin/AnyCPU/Release/TQVaultAE.exe).VersionInfo.FileVersion
(Get-Item src/TQSaveFilesExplorer/bin/Release/TQ.SaveFilesExplorer.exe).VersionInfo.FileVersion
(Get-Item src/ARZExplorer/bin/Release/ARZExplorer.exe).VersionInfo.FileVersion
```

**Check compiled DLLs (.NET Standard 2.0):**
```bash
# Windows PowerShell
(Get-Item src/TQVaultAE.Domain/bin/Release/netstandard2.0/TQVaultAE.Domain.dll).VersionInfo.FileVersion
(Get-Item src/TQVaultAE.Services/bin/Release/netstandard2.0/TQVaultAE.Services.dll).VersionInfo.FileVersion
(Get-Item src/TQVaultAE.Presentation/bin/Release/netstandard2.0/TQVaultAE.Presentation.dll).VersionInfo.FileVersion
(Get-Item src/TQVaultAE.Data/bin/Release/netstandard2.0/TQVaultAE.Data.dll).VersionInfo.FileVersion
(Get-Item src/TQVaultAE.Config/bin/Release/netstandard2.0/TQVaultAE.Config.dll).VersionInfo.FileVersion
(Get-Item src/TQVaultAE.Logs/bin/Release/netstandard2.0/TQVaultAE.Logs.dll).VersionInfo.FileVersion
```

**Or check .csproj files directly:**
```bash
# Check Version in all .NET SDK-style projects
grep "<Version>" src/*/*.csproj
```

Expected output (all lines show same version):
```
src/TQVaultAE.Domain/TQVaultAE.Domain.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Services/TQVaultAE.Services.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Presentation/TQVaultAE.Presentation.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Data/TQVaultAE.Data.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Config/TQVaultAE.Config.csproj:    <Version>4.4.1.0</Version>
src/TQVaultAE.Logs/TQVaultAE.Logs.csproj:    <Version>4.4.1.0</Version>
```

All executables and DLLs should return identical version strings.

### Committing Version Changes

**Important**: The workflow automatically commits version changes to `main` branch only.

- **Main branch**: Version auto-committed by GitHub Actions bot on release
- **Pull requests**: No version changes committed
- **Feature branches**: No version changes committed

This prevents merge conflicts and ensures clean history.

### Git Tagging on Release

When a PR is merged to `main`, the workflow automatically creates a git tag with the version number in format `Major.Minor.Build` (e.g., `4.5.0`).

**Tag Format:**
- Tags follow semantic versioning format: `Major.Minor.Build`
- Example: If version is `4.5.0.0`, the tag will be `4.5.0`
- Tags are lightweight and point to the commit with the version bump

**When Tags Are Created:**
- Only on merges to `main` branch
- After version files are updated and committed
- Tag is pushed to origin immediately after creation

**Viewing Tags:**
```bash
# List all tags
git tag

# View tag details
git show 4.5.0

# Push tags manually (if needed)
git push origin --tags
```

**Benefits:**
- Easy reference to specific releases
- Quick rollback to previous versions
- Clear release history in git
- Works with GitHub's tag-based release navigation

### Version Increment Flow Example

```
Initial state: 4.4.0.0 (all 10 files)

PR #1 merged to main:
  Update-Version.ps1 -VersionType "Release"
  → 4.5.0.0 (all 10 files synced)
  → Git tag "4.5.0" created and pushed

PR #2 merged to main:
  Update-Version.ps1 -VersionType "Release"
  → 4.6.0.0 (all 10 files synced)
  → Git tag "4.6.0" created and pushed

PR #3 merged to main:
  Update-Version.ps1 -VersionType "Release"
  → 4.7.0.0 (all 10 files synced)
  → Git tag "4.7.0" created and pushed
```

**After each release:**
- `TQVaultAE.GUI/Properties/AssemblyInfo.cs` → 4.5.0.0
- `TQSaveFilesExplorer/Properties/AssemblyInfo.cs` → 4.5.0.0
- `ARZExplorer/Properties/AssemblyInfo.cs` → 4.5.0.0
- `TQVaultAE.Services.Win32/Properties/AssemblyInfo.cs` → 4.5.0.0
- `TQVaultAE.Domain/TQVaultAE.Domain.csproj` → 4.5.0.0
- `TQVaultAE.Services/TQVaultAE.Services.csproj` → 4.5.0.0
- `TQVaultAE.Presentation/TQVaultAE.Presentation.csproj` → 4.5.0.0
- `TQVaultAE.Data/TQVaultAE.Data.csproj` → 4.5.0.0
- `TQVaultAE.Config/TQVaultAE.Config.csproj` → 4.5.0.0
- `TQVaultAE.Logs/TQVaultAE.Logs.csproj` → 4.5.0.0
- `version-info.json` → 4.5.0.0

## Additional Resources

- GitHub Actions Documentation: https://docs.github.com/en/actions
- .NET Framework on GitHub Actions: https://docs.github.com/en/actions/automating-builds-and-tests/building-and-testing-net
- MSBuild Command Line Reference: https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-command-line-reference
- xUnit Documentation: https://xunit.net/docs/getting-started/netcore/cmdline
