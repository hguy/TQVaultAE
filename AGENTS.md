# TQVaultAE Agent Guidelines

Guidelines for AI agents working on the TQVaultAE codebase.

## Build, Test, and Lint Commands

### Build
```bash
dotnet build TQVaultAE.sln
dotnet build src/TQVaultAE.Services/TQVaultAE.Services.csproj
dotnet build TQVaultAE.sln --configuration Release
dotnet restore
```

### Test
```bash
# Run all tests
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj

# Run single test
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --filter "FullyQualifiedName~ClassName.TestName"

# Run specific test class
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --filter "FullyQualifiedName~TQVaultAE.Tests.Services.GameFileServiceTests"

# Run tests with coverage
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Code Style Guidelines

### Formatting
- **Indentation**: Tabs (configured in .editorconfig)
- **Braces**: Mandatory (K&R style)
- **Language Version**: C# 10.0
- **No comments** in code (unless explicitly requested)

### Naming Conventions
- **Classes/Interfaces**: PascalCase (`ItemService`, `IItemService`)
- **Methods**: PascalCase (`GetItemById()`)
- **Properties**: PascalCase (`ItemName`)
- **Local Variables**: camelCase (`itemCount`)
- **Private Fields**: camelCase with underscore prefix (`_itemCount`)
- **Constants**: PascalCase (`MaxItems`, `MAINVAULT`)
- **Interfaces**: Prefix with `I`

### File Structure
```csharp
namespace ProjectNamespace
{
    using System;
    using Other.Namespaces;

    public class ClassName
    {
        private readonly IService _service;
        private const string CONSTANT = "value";

        public ClassName(IService service)
        {
            _service = service;
        }

        public void MethodName()
        {
        }
    }
}
```

### Import Ordering
1. System namespaces
2. Third-party library namespaces (e.g., Microsoft, Newtonsoft, Moq)
3. Project-specific namespaces (e.g., TQVaultAE.Domain)

### Error Handling
- Validate method parameters at entry
- Use exceptions for exceptional cases
- Log errors using `ILogger<T>`
- Use `try-catch` blocks appropriately

### Dependency Injection
- Use constructor injection for all service dependencies
- Register services in `src/TQVaultAE.GUI/Program.cs`
- Mock dependencies in tests using Moq

### Abstractions Pattern
- Use IFileIO and IPathIO for file/path operations (enables testability)
- Store abstractions as private readonly fields

## Test Guidelines

### Test Framework
- xUnit with AwesomeAssertions for fluent assertions
- Moq for mocking dependencies

### Test Structure
```csharp
public class ServiceTests
{
    private readonly Mock<ILogger<Service>> _mockLogger;
    private readonly Mock<IService> _mockService;
    private readonly Service _service;

    public ServiceTests()
    {
        _mockLogger = new Mock<ILogger<Service>>();
        _mockService = new Mock<IService>();
        _service = new Service(_mockLogger.Object, _mockService.Object);
    }

    [Fact]
    public void Method_WithCondition_ReturnsExpected()
    {
        var input = "test";
        var result = _service.Method(input);
        result.Should().Be("expected");
        _mockService.Verify(x => x.SomeMethod(), Times.Once);
    }
}
```

### Testing Best Practices
- Follow Arrange-Act-Assert pattern
- Test edge cases including null inputs
- Use descriptive test names (`Method_WithCondition_ReturnsExpected`)
- Verify mock calls when dependencies are used
- Mock IFileIO/IPathIO methods in tests if they're called

## Project Structure

### Layer Dependencies
GUI → Presentation → Services → Domain → Data

### Key Projects
- **TQVaultAE.GUI**: Windows Forms application (main entry point)
- **TQVaultAE.Domain**: Entities, contracts, interfaces
- **TQVaultAE.Services**: Business logic and service implementations
- **TQVaultAE.Data**: Data access and persistence
- **TQVaultAE.Tests**: Unit tests (xUnit)

## Common Tasks

### Adding a Service
1. Define interface in `TQVaultAE.Domain/Contracts/Services/`
2. Implement in `TQVaultAE.Services/`
3. Add IFileIO/IPathIO dependencies for file/path operations
4. Register in `Program.cs` DI container
5. Add unit tests in `TQVaultAE.Tests/Services/`

### Adding an Abstraction
Follow IFileIO/IPathIO pattern:
- Create interface in `Domain/Contracts/Services/`
- Implement in `Services/` delegating to static methods
- Register in `Program.cs`
- Replace static calls with abstraction calls in services
- Update tests to mock the abstraction

## Key Dependencies & Resources
- **Newtonsoft.Json**: JSON serialization
- **Microsoft.Extensions.Logging**: Logging infrastructure
- **Magick.NET-Q8-AnyCPU**: DDS to PNG conversion (requires ImageMagick installed via `winget install ImageMagick.Q8`)
- **xUnit**: Testing framework
- **Moq**: Mocking for unit tests
- **AwesomeAssertions**: Fluent assertions for tests
- GitHub: https://github.com/EtienneLamoureux/TQVaultAE
- Issues: https://github.com/EtienneLamoureux/TQVaultAE/issues