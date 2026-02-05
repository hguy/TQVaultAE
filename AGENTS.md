# TQVaultAE Agent Guidelines

## Build Commands

```bash
# Build entire solution (modern slim format - Any CPU only)
dotnet build TQVaultAE.slnx
dotnet build TQVaultAE.slnx --configuration Release

# Build with specific platform (use classic format)
dotnet build TQVaultAE.sln --configuration Release -p:Platform=x64
dotnet build TQVaultAE.sln --configuration Release -p:Platform=x86

# Build specific project
dotnet build src/TQVaultAE.GUI/TQVaultAE.GUI.csproj

# Restore packages
dotnet restore
```

**Solution Files:**
- `TQVaultAE.slnx` - Modern slim format (XML, ~80% smaller, Any CPU builds only)
- `TQVaultAE.sln` - Classic format (full platform support: AnyCPU/x64/x86)

## Test Commands

```bash
# Run all tests
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj

# Run specific test class
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --filter "FullyQualifiedName~TagServiceTests"

# Run single test method
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --filter "FullyQualifiedName~TagServiceTests.AddTag_WithNewTagName_ReturnsTrue"

# Run with coverage
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Code Style

### Formatting
- **Indentation**: Tabs (see `.editorconfig`)
- **Language**: C# 10.0 with file-scoped namespaces
- **Braces**: Required, K&R style
- **This**: Use `this.` prefix for instance members (e.g., `this.TagConfig`)
- **Comments**: Minimize comments unless requested

### Naming
- **Classes/Interfaces**: PascalCase (`ItemService`, `IItemService`)
- **Methods/Properties**: PascalCase (`GetItemById()`, `ItemName`)
- **Variables**: camelCase (`itemCount`)
- **Private fields**: camelCase with underscore (`_itemCount`)
- **Constants**: PascalCase (`MaxItems`)
- **Interfaces**: Prefix with `I`

### Import Ordering
1. System namespaces
2. Third-party libraries (Microsoft, Moq)
3. Project namespaces (`TQVaultAE.Domain`)

### File Structure
```csharp
using System;
using Microsoft.Extensions.Logging;
using TQVaultAE.Domain.Contracts.Services;

namespace TQVaultAE.Services;

public class MyService : IMyService
{
    private readonly ILogger _logger;
    private readonly IDataService _dataService;

    public MyService(ILogger<MyService> logger, IDataService dataService)
    {
        this._logger = logger;
        this._dataService = dataService;
    }

    public void DoWork()
    {
    }
}
```

## Testing

### Framework
- **xUnit** with **AwesomeAssertions** (fluent assertions)
- **Moq** for mocking

### Test Pattern
```csharp
using AwesomeAssertions;
using Moq;

namespace TQVaultAE.Tests.Services;

public class ServiceTests
{
    private readonly Mock<ILogger<Service>> _mockLogger;
    private readonly Service _service;

    public ServiceTests()
    {
        _mockLogger = new Mock<ILogger<Service>>();
        _service = new Service(_mockLogger.Object);
    }

    [Fact]
    public void Method_WithCondition_ReturnsExpected()
    {
        // Arrange
        var input = "test";

        // Act
        var result = _service.Method(input);

        // Assert
        result.Should().Be("expected");
        _mockLogger.Verify(x => x.LogInformation(It.IsAny<string>()), Times.Once);
    }
}
```

## Architecture

### Layer Dependencies
```
GUI → Presentation → Services → Domain → Data
```

### Projects
- **TQVaultAE.GUI**: Windows Forms executable (.NET Framework 4.8, SDK-style)
- **ARZExplorer**: Windows Forms executable (.NET Framework 4.8, SDK-style)
- **TQ.SaveFilesExplorer**: Windows Forms executable (.NET Framework 4.8, SDK-style)
- **TQVaultAE.Domain**: Entities, contracts, interfaces (.NET Standard 2.0)
- **TQVaultAE.Services**: Business logic (.NET Standard 2.0)
- **TQVaultAE.Services.Win32**: Windows-specific services (.NET Framework 4.8, SDK-style)
- **TQVaultAE.Data**: Data access (.NET Standard 2.0)
- **TQVaultAE.Presentation**: Presentation layer (.NET Standard 2.0)
- **TQVaultAE.Config**: Configuration (.NET Standard 2.0)
- **TQVaultAE.Logs**: Logging (.NET Standard 2.0)
- **TQVaultAE.Tests**: Unit tests (xUnit, .NET 10.0)

### Dependency Injection
- Constructor injection for all services
- Register in `src/TQVaultAE.GUI/Program.cs`
- Use `IFileIO` and `IPathIO` abstractions for file operations (enables testing)

## Version Management

See [VERSIONING.md](VERSIONING.md) for details. Key points:
- All 10 projects must share the same version (4 EXEs + 6 DLLs)
- Source of truth: `version-info.json`
- CI/CD auto-increments Minor version on `main` branch push
- **Never** manually edit `.csproj` version fields - use the version sync script

## Common Tasks

### Adding a Service
1. Create interface in `TQVaultAE.Domain/Contracts/Services/`
2. Implement in `TQVaultAE.Services/`
3. Use `IFileIO`/`IPathIO` for file operations
4. Register in `Program.cs`
5. Add tests in `TQVaultAE.Tests/Services/`

## Dependencies
- **System.Text.Json**: JSON serialization
- **Microsoft.Extensions.Logging**: Logging
- **Magick.NET-Q8-AnyCPU**: Image processing (requires ImageMagick)
- **xUnit**: Testing
- **Moq**: Mocking
- **AwesomeAssertions**: Assertions

## Resources
- GitHub: https://github.com/EtienneLamoureux/TQVaultAE
- Issues: https://github.com/EtienneLamoureux/TQVaultAE/issues
