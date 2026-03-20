# TQVaultAE Agent Guidelines

This document provides guidelines for AI coding agents working in the TQVaultAE codebase.

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

# Run with OpenCover coverage
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --collect:"XPlat Code Coverage;Format=opencover"

# Build Debug before running tests
dotnet build src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --configuration Debug
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --no-build

# List all tests
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --list-tests
```

## Code Style

### Formatting
- **Indentation**: Tabs (per `.editorconfig`)
- **Language**: C# 10.0 with file-scoped namespaces
- **Braces**: Required, K&R style (opening brace on same line)
- **This Prefix**: Use `this.` prefix for all instance members (e.g., `this.TagConfig`, `this._logger`)
- **Comments**: Minimize comments unless requested; prefer self-documenting code

### Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Classes/Interfaces | PascalCase | `ItemService`, `IItemService` |
| Methods/Properties | PascalCase | `GetItemById()`, `ItemName` |
| Variables | camelCase | `itemCount`, `playerSave` |
| Private fields | camelCase with underscore | `_itemCount`, `_playerService` |
| Constants | PascalCase | `MaxItems`, `DefaultTimeout` |
| Interfaces | Prefix with `I` | `IFileIO`, `IStashService` |

### Import Ordering
1. System namespaces (`System`, `System.Collections.Generic`)
2. Third-party libraries (`Microsoft`, `Moq`, `xUnit`)
3. Project namespaces (`TQVaultAE.Domain`, `TQVaultAE.Application`)

### Nullable Reference Types
- Enable nullable reference types: `<Nullable>enable</Nullable>` in projects
- Use `?` for nullable parameters and return types
- Use `not null` constraints where appropriate
- Initialize collections as empty rather than null when possible

### Async/Await Patterns
- Use `async`/`await` for I/O operations
- Do not block on async code (avoid `.Result`, `.Wait()`)
- Use `Task.FromResult()` for synchronous fallbacks in async interfaces
- Name async methods with `Async` suffix: `LoadPlayerAsync()`

### Error Handling
- Prefer Result types over exceptions for expected failure cases
- Use `ArgumentNullException` for required null parameters
- Log exceptions with context: `Log.LogError(ex, "Failed to load player {Name}", playerName)`
- Wrap file I/O in try-catch with user-friendly error messages

## Testing

### Framework
- **xUnit** for test framework
- **AwesomeAssertions** for fluent assertions
- **Moq** for mocking

### Test File Location
- Place tests in `src/TQVaultAE.Tests/Services/`
- Name test files: `<ServiceName>Tests.cs`

### Test Pattern
```csharp
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TQVaultAE.Application;
using TQVaultAE.Application.Contracts.Services;

namespace TQVaultAE.Tests.Services;

public class ServiceTests
{
    private readonly Mock<ILogger<Service>> _mockLogger;
    private readonly Mock<IFileIO> _mockFileIO;
    private readonly Service _service;

    public ServiceTests()
    {
        _mockLogger = new Mock<ILogger<Service>>();
        _mockFileIO = new Mock<IFileIO>();
        _service = new Service(_mockLogger.Object, _mockFileIO.Object);
    }

    [Fact]
    public void Method_WithCondition_ReturnsExpected()
    {
        // Arrange
        var input = "test";
        _mockFileIO.Setup(x => x.Exists("test.txt")).Returns(true);

        // Act
        var result = _service.Method(input);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo("expected");
        _mockFileIO.Verify(x => x.Exists(It.IsAny<string>()), Times.Once);
    }
}
```

### Cross-Platform Testing
- Use Unix-style paths (`/Test/Players/`) for test paths
- Mock `IPathIO` to control path parsing behavior
- Create real temp directories for `FileSystemWatcher` tests

## Architecture

### Layer Dependencies
```
GUI → Presentation → Services → Domain → Data
     ↓
Application (DTOs, Results, Search)
```

### Projects
| Project | Framework | Purpose |
|---------|-----------|---------|
| TQVaultAE.GUI | .NET 10.0 | Windows Forms UI |
| TQVaultAE.Domain | .NET 10.0 | Entities, interfaces |
| TQVaultAE.Services | .NET 10.0 | Business logic |
| TQVaultAE.Application | .NET 10.0 | DTOs, Results, Search |
| TQVaultAE.Data | .NET 10.0 | Data access |
| TQVaultAE.Tests | .NET 10.0 | Unit tests |
| ARZExplorer | .NET 10.0 | ARZ/ARC file extractor |
| TQSaveFilesExplorer | .NET Framework 4.8 | Save file explorer |

### Dependency Injection
- Constructor injection for all services
- Register in `src/TQVaultAE.GUI/Program.cs`
- Use `IFileIO` and `IPathIO` abstractions for file operations (enables testing)

### Key Abstractions
- `IFileIO` - File read/write operations
- `IPathIO` - Path manipulation operations
- `IGamePathService` - Game path resolution
- `SessionContext` - Shared application state

## Common Tasks

### Adding a Service
1. Create interface in `src/TQVaultAE.Application/Contracts/Services/`
2. Implement in `src/TQVaultAE.Services/`
3. Use `IFileIO`/`IPathIO` for file operations
4. Register in `src/TQVaultAE.GUI/Program.cs`
5. Add tests in `src/TQVaultAE.Tests/Services/`

### Adding a Test
1. Create test class in `src/TQVaultAE.Tests/Services/`
2. Mock all dependencies in constructor
3. Use `CreateItem()` or similar helpers for test data
4. Follow Arrange-Act-Assert pattern

## Dependencies
| Package | Purpose |
|---------|---------|
| xUnit | Test framework |
| Moq | Mocking |
| AwesomeAssertions | Fluent assertions |
| Microsoft.Extensions.Logging | Logging abstraction |
| System.Text.Json | JSON serialization |
| EnumsNET | Enum operations |
| Magick.NET | Image processing |

## Resources
- GitHub: https://github.com/EtienneLamoureux/TQVaultAE
- Issues: https://github.com/EtienneLamoureux/TQVaultAE/issues
