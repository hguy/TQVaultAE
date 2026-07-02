# TQVaultAE Agent Guidelines

## Build Commands

```bash
# Build solution (Any CPU)
dotnet build TQVaultAE.slnx
dotnet build TQVaultAE.slnx --configuration Release

# Build specific project
dotnet build src/TQVaultAE.GUI/TQVaultAE.GUI.csproj

# Restore packages
dotnet restore
```

**Solution File:** `TQVaultAE.slnx` (modern .NET 10.0 SDK-style format)

## Test Commands

```bash
# Run all tests
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj

# Run single test class
dotnet test --filter "FullyQualifiedName~TagServiceTests"

# Run single test method
dotnet test --filter "FullyQualifiedName~TagServiceTests.AddTag_WithNewTagName_ReturnsTrue"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage;Format=opencover"

# Build before testing
dotnet build src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --configuration Debug
dotnet test src/TQVaultAE.Tests/TQVaultAE.Tests.csproj --no-build
```

## Code Style

### Formatting
- **Indentation**: Tabs
- **Braces**: K&R style (opening brace on same line)
- **Namespaces**: File-scoped
- **This Prefix**: Use `this.` for all instance members

### Naming Conventions
| Element | Convention | Example |
|---------|------------|---------|
| Classes/Interfaces | PascalCase | `ItemService`, `IItemService` |
| Private fields | camelCase with underscore | `_itemCount` |
| Interfaces | Prefix with `I` | `IFileIO` |

### Types & Nullable
- Enable nullable reference types: `<Nullable>enable</Nullable>`
- Use `?` for nullable parameters
- Initialize collections as empty rather than null

### String Literals
- Use C# raw string literals (`"""..."""`) for:
  - JSON strings (replaces `\"` escaping)
  - File paths (replaces `\\` or `@""` escaping)
  - Multi-line strings with `\n`/`\t`
- Keep regular `"..."` for short InlineData parameters and trivial single-word strings

### Async Patterns
- Use `async`/`await` for I/O operations
- Name async methods with `Async` suffix: `LoadPlayerAsync()`
- Do not block on async code (avoid `.Result`, `.Wait()`)

### Error Handling
- Prefer Result types over exceptions for expected failures
- Log exceptions with context: `Log.LogError(ex, "Failed to load {Name}", name)`

## Testing

### Framework
- **xUnit** for tests, **AwesomeAssertions** for assertions, **Moq** for mocking

### What is a Meaningful Test? ⚠️ IMPORTANT

**AVOID trivial tests that add NO value:**
- ❌ Property getter/setter without logic (`*_CanBeSet`)
- ❌ Default value tests without behavior
- ❌ Constructor verification (`new Class()`)
- ❌ `x.Should().Be(y)` with constant values

**WRITE meaningful tests that verify:**
- ✅ Business logic and calculations
- ✅ Error handling and edge cases
- ✅ Interactions between components
- ✅ State transitions and side effects

**Rule:** If you can't answer "What bug would this test catch?" → DELETE it

### Cross-Platform Testing
- Use `Path.GetTempPath()` for paths
- Mock `IPathIO` for path control
- DirectoryNotFoundException handling differs on Linux

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
| TQVaultAE.Services | .NET 10.0 | Business logic |
| TQVaultAE.Application | .NET 10.0 | DTOs, Results |
| TQVaultAE.Data | .NET 10.0 | Data access |
| TQVaultAE.Domain | .NET 10.0 | Entities |
| TQVaultAE.Tests | .NET 10.0 | Unit tests |

### Key Abstractions (for DI)
- `IFileIO` - File operations
- `IPathIO` - Path operations
- `IGamePathService` - Game path resolution
- Register services in `src/TQVaultAE.GUI/Program.cs`

## Adding New Code

### Adding a Service
1. Create interface in `src/TQVaultAE.Application/contracts/Services/`
2. Implement in `src/TQVaultAE.Services/`
3. Use `IFileIO`/`IPathIO` for file operations
4. Register in `Program.cs`
5. Add tests with meaningful assertions

### Adding a Test
1. Create test class: `src/TQVaultAE.Tests/<Category>/<Name>Tests.cs`
2. Mock all dependencies in constructor
3. Follow Arrange-Act-Assert pattern
4. Verify actual behavior, not property assignments

## Avalonia Development Tools

### Skill
- **`avalonia` skill** — loaded via `.agents/skills/avalonia/SKILL.md` with `skill({ name: "avalonia" })`. Covers MVVM, XAML, data binding, styling, theming, custom controls, platform-specific. Resources for deep dives: mvvm-databinding, controls-reference, styling-guide, reactive-animations, custom-controls-advanced, platform-specific.

### MCP Servers (configured in opencode.jsonc)

#### `codebase-memory-mcp` — Code Intelligence (Always Use First)
Indexed project: **D-GitRepo-TQVaultAE** (10,675 nodes, 31,087 edges).

**Before** writing any code or searching with grep/glob, use these tools:
- `search_graph(query="...", project="D-GitRepo-TQVaultAE")` — natural language search for functions, classes, routes. Beats grep for discovery.
- `trace_path(function_name="...", project="D-GitRepo-TQVaultAE")` — follow callers/callees, data flow, cross-service calls. Use for impact analysis before modifying code.
- `get_architecture(project="D-GitRepo-TQVaultAE")` — overview of packages, services, dependencies, and Leiden clusters (de-facto modules).
- `query_graph(query="...", project="D-GitRepo-TQVaultAE")` — arbitrary Cypher queries for complex patterns (e.g. high cyclomatic complexity, deep nesting, dead code).
- `search_code(pattern="...", project="D-GitRepo-TQVaultAE")` — grep enriched with graph context, deduplicated by function.
- `get_code_snippet(qualified_name="...", project="D-GitRepo-TQVaultAE")` — read source by qualified name from search_graph results.

**Workflow**: search_graph → get_code_snippet → trace_path (modify) → verify.

#### `avalonia-ui-mcp` — XAML & Project Tooling (Docker required)
Use for:
- `ValidateXaml(xamlContent)` — validate XAML syntax before committing
- `GenerateTheme(...)` / `GenerateAnimation(...)` — scaffolding
- `ConvertWpfXamlToAvalonia(...)` — migration from WPF
- `GenerateResponsiveDesign(...)` / `GenerateCustomControl(...)` — code generation

#### `avalonia-mcp` — Runtime Debugger (App must be running)
`AvaloniaMcp.Diagnostics` NuGet package + `.UseMcpDiagnostics()` already wired in `Program.cs` under `#if DEBUG`.

Global tool installed: `avalonia-mcp` v0.4.0.

Use after launching the app:
```
avalonia-mcp cli list_windows
avalonia-mcp cli get_visual_tree --maxDepth 5
avalonia-mcp cli get_binding_errors
avalonia-mcp cli get_data_context --controlId "#MyControl"
avalonia-mcp cli take_screenshot
```

## Available Subagents

These specialist agents are available as primary (Tab key) or subagent (`task()`) and are defined in `.opencode/agents/`. Use for complex, focused work:

### `dotnet-contribution-dotnet-architect`
Expert .NET backend architect for C#, ASP.NET Core, EF Core, Dapper, DI, and performance. Use when:
- Designing new services or refactoring existing ones
- Complex DI registration decisions
- Performance optimization of .NET code
- API design and data access patterns

### `code-documentation-code-reviewer`
Elite code review for security, performance, architecture, testing, and accessibility. Use when:
- Reviewing PRs or large changes before commit
- Auditing code quality across the project
- Need structured finding reports with severity tiers

### `error-debugging-debugger`
Systematic debugger for errors, test failures, and unexpected behavior. Use when:
- A test is failing and root cause isn't obvious
- Investigating runtime exceptions or logic bugs
- Need step-by-step reproduction and fix

### `backend-development-test-automator`
Creates comprehensive test suites (unit, integration, E2E). Use when:
- Adding tests for new features
- Improving coverage in existing code
- Need test fixtures, mocks, or builders

## Resources
- GitHub: https://github.com/EtienneLamoureux/TQVaultAE
- Issues: https://github.com/EtienneLamoureux/TQVaultAE/issues
