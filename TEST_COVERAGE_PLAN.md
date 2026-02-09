# TQVaultAE Code Coverage Improvement Plan

## Current State Analysis

### Codebase Statistics
- **Total C# source files**: 271
- **Test files**: 1 (UnitTest1.cs - mostly empty) + GameFileServiceTests.cs (new)
- **Production code files**: 270
- **Current test coverage**: Minimal to nonexistent (but improving)
- **Active test infrastructure**: xUnit + Moq + AwesomeAssertions + Coverlet

### Key Observations
1. **Single empty test file**: The test project exists but contains only an empty test
2. **Multiple layers**: Domain, Services, Data, GUI, and utility projects
3. **Complex functionality**: Item management, vault operations, game file parsing
4. **External dependencies**: Game file formats, database operations

## Phase 1: Test Infrastructure Setup (Week 1-2)

### Goals
- Establish proper test infrastructure
- Create testable interfaces and mocks
- Set up CI/CD integration for coverage reporting

### Tasks

#### 1.1 Test Project Enhancement
- [x] Add proper test project structure with folders:
  - `UnitTests/` - for isolated unit tests
  - `IntegrationTests/` - for multi-component tests
  - `Mocks/` - for mock implementations
  - `TestData/` - for test fixtures and data
- [x] Create `Services/` folder for service layer tests
- [x] Implement GameFileServiceTests.cs as reference implementation

#### 1.2 Mocking Framework Setup
- [x] Enhance Moq usage for complex dependencies
- [x] Create mock implementations for key interfaces:
  - `IGamePathService` mock (✅ implemented)
  - `ILogger<T>` mock (✅ implemented)
  - `IUIService` mock (✅ implemented)
  - `ITranslationService` mock (✅ implemented)
- [ ] Create additional mocks for:
  - `IDatabase` mock
  - `IItemProvider` mock  
  - `IPlayerCollectionProvider` mock
  - File system mocks for game file operations

#### 1.3 CI/CD Integration
- [ ] Add GitHub Actions workflow for test execution
- [ ] Configure coverage reporting with Coveralls or Codecov
- [ ] Set up test execution on pull requests

#### 1.4 Test Utilities
- [ ] Create test helpers for common scenarios
- [ ] Implement test data builders for Domain entities
- [ ] Add assertion helpers for complex object comparisons

## Phase 2: Core Domain Layer Testing (Week 3-5)

### Goals
- Achieve 80%+ coverage on domain layer
- Test all business logic and validation rules
- Ensure domain entities are properly validated

### Priority Areas

#### 2.1 Domain Entities Testing
- [ ] Test all entity validation logic
- [ ] Test entity creation and property access
- [ ] Test serialization/deserialization
- [ ] Test equality and comparison logic

#### 2.2 Domain Services Testing
- [ ] Test business rule validation
- [ ] Test domain service methods
- [ ] Test edge cases and error conditions
- [ ] Test domain event handling

#### 2.3 Value Objects Testing
- [ ] Test value object creation and validation
- [ ] Test immutability constraints
- [ ] Test equality comparisons

## Phase 3: Data Layer Testing (Week 6-8)

### Goals
- Achieve 75%+ coverage on data access layer
- Test all repository operations
- Ensure proper error handling

### Priority Areas

#### 3.1 Repository Testing
- [ ] Test `ArcFileProvider` methods
- [ ] Test `ArzFileProvider` methods
- [ ] Test `Database` operations
- [ ] Test `ItemAttributeProvider` logic

#### 3.2 Data Access Testing
- [ ] Test CRUD operations
- [ ] Test query methods
- [ ] Test error handling for file operations
- [ ] Test data transformation logic

#### 3.3 Integration Testing
- [ ] Test database connection handling
- [ ] Test file I/O operations
- [ ] Test data caching mechanisms

## Phase 4: Service Layer Testing (Week 9-11)

### Goals
- Achieve 70%+ coverage on service layer
- Test all service methods and workflows
- Ensure proper exception handling

### Priority Areas

#### 4.1 Core Service Testing
- [x] Test `GameFileService` methods (✅ 7 tests implemented)
  - `Archive_WithNullPlayerSave_ReturnsFalse`
  - `Archive_WithAlreadyArchivedPlayerSave_ReturnsTrue`
  - `Unarchive_WithNullPlayerSave_ReturnsFalse`
  - `Unarchive_WithAlreadyUnarchivedPlayerSave_ReturnsTrue`
  - `GitAddCommitTagAndPush_WithDisabledBackup_ReturnsFalse`
  - `BackupStupidPlayerBackupFolder_HandlesExistingBackupFolder`
  - `GitRepositorySetup_WithDisabledBackup_DoesNothing`
- [🔲] Test `PlayerService` operations (🔲 11 tests created, need fixes for complex scenarios)
  - `LoadPlayer_WithNullPlayerSave_ReturnsEmptyResult`
  - `LoadPlayer_WithEmptyPlayerSaveName_ReturnsEmptyResult`  
  - `LoadPlayer_WithValidPlayerSave_LoadsSuccessfully`
  - `LoadPlayer_WithArgumentException_HandlesGracefully`
  - `LoadPlayer_FromFileWatcher_UsesAddOrUpdateAtomic`
  - `SaveAllModifiedPlayers_WithNoModifiedPlayers_ReturnsFalse`
  - `SaveAllModifiedPlayers_WithModifiedPlayer_SavesSuccessfully`
  - `SaveAllModifiedPlayers_WithModifiedPlayerAndEnabledBackup_BackupsAndSavesSuccessfully`
  - `GetPlayerSaveList_WithCharacters_ReturnsSortedList`
  - `GetPlayerSaveList_WithTQITCharacter_SetsIsImmortalThrone`
  - `GetPlayerSaveList_WithArchivedCharacter_SetsIsArchived`
  - `GetPlayerSaveList_WithCustomCharacter_SetsIsCustom`
  - `GetPlayerSaveList_WithEmptyList_ReturnsEmptyArray`
  - `AlterNameInPlayerFileSave_WithValidData_CallsTQDataService`
- [ ] Test `StashService` functionality
- [ ] Test `VaultService` methods

#### 4.2 Service Workflow Testing
- [ ] Test complex workflows involving multiple services
- [ ] Test transaction handling
- [ ] Test error recovery scenarios
- [ ] Test performance-critical operations

#### 4.3 Dependency Injection Testing
- [ ] Test service registration
- [ ] Test dependency resolution
- [ ] Test lifecycle management

## Phase 5: Utility and Helper Testing (Week 12-13)

### Goals
- Achieve 65%+ coverage on utility classes
- Test all helper methods and extensions

### Priority Areas

#### 5.1 Utility Class Testing
- [ ] Test extension methods
- [ ] Test helper classes
- [ ] Test conversion utilities
- [ ] Test validation helpers

#### 5.2 Configuration Testing
- [ ] Test configuration loading
- [ ] Test configuration validation
- [ ] Test configuration merging

## Phase 6: Integration and End-to-End Testing (Week 14-16)

### Goals
- Achieve 60%+ coverage on integration scenarios
- Test key user workflows
- Ensure system components work together

### Priority Areas

#### 6.1 Multi-Component Testing
- [ ] Test service + repository interactions
- [ ] Test domain + service workflows
- [ ] Test error propagation across layers

#### 6.2 End-to-End Scenarios
- [ ] Test item management workflows
- [ ] Test player character operations
- [ ] Test vault management scenarios
- [ ] Test game file import/export

## Phase 7: Test Maintenance and Optimization (Ongoing)

### Goals
- Maintain high coverage levels
- Optimize test execution time
- Ensure tests remain relevant

### Tasks

#### 7.1 Test Refactoring
- [ ] Identify and remove duplicate tests
- [ ] Optimize slow-running tests
- [ ] Improve test organization

#### 7.2 Coverage Monitoring
- [ ] Set up coverage alerts for drops below thresholds
- [ ] Regular coverage reviews
- [ ] Identify untested code areas

#### 7.3 Test Documentation
- [ ] Document test strategies
- [ ] Add comments for complex test scenarios
- [ ] Maintain test coverage reports

## Testing Strategy by Component

### Domain Layer Strategy
- **Focus**: Business logic, validation rules, entity behavior
- **Approach**: Isolated unit tests with mock dependencies
- **Coverage Target**: 80%+

### Service Layer Strategy  
- **Focus**: Service methods, workflows, exception handling
- **Approach**: Unit tests with mocked repositories, integration tests
- **Coverage Target**: 70%+

### Data Layer Strategy
- **Focus**: Repository operations, query methods, error handling
- **Approach**: Unit tests with test databases, integration tests
- **Coverage Target**: 75%+

### GUI Layer Strategy
- **Focus**: View model logic, command handling
- **Approach**: View model unit tests, limited UI automation
- **Coverage Target**: 50%+

## Recommended Test Types

### Unit Tests
- Isolated component testing
- Fast execution
- Mock all external dependencies
- Test individual methods and classes

### Integration Tests
- Multi-component testing
- Test component interactions
- Use real implementations where possible
- Test workflows and data flows

### End-to-End Tests
- Full system testing
- Test complete user scenarios
- Validate system behavior
- Limited scope due to complexity

## Test Coverage Targets

| Component | Target Coverage | Priority |
|-----------|----------------|----------|
| Domain Layer | 80%+ | High |
| Data Layer | 75%+ | High |
| Service Layer | 70%+ | High |
| Utility Classes | 65%+ | Medium |
| GUI/VM Layer | 50%+ | Medium |
| Integration | 60%+ | High |

## Implementation Timeline

```
Week 1-2: Test Infrastructure Setup
Week 3-5: Domain Layer Testing
Week 6-8: Data Layer Testing  
Week 9-11: Service Layer Testing
Week 12-13: Utility Testing
Week 14-16: Integration Testing
Ongoing: Test Maintenance
```

## Success Metrics

1. **Overall Coverage**: Increase from ~0% to 65%+ within 4 months
2. **Domain Coverage**: 80%+ coverage on core business logic
3. **Critical Path Coverage**: 100% coverage on most used features
4. **Test Reliability**: 95%+ test pass rate (✅ Currently 100% for GameFileService)
5. **Test Execution Time**: Under 5 minutes for full test suite (✅ Currently ~173ms for GameFileService tests)

## Current Progress (Updated)

### ✅ Completed Items
- **Test Infrastructure**: xUnit + Moq + AwesomeAssertions + Coverlet fully configured
- **Service Layer Testing**: 
  - **GameFileService**: ✅ 7 comprehensive tests covering:
    - Null parameter handling
    - Configuration-based behavior  
    - Early return optimizations
    - File system operations
    - Git repository operations
  - **PlayerService**: 🔲 11 tests created (need fixes for complex scenarios):
    - Basic load operations and validation
    - Player save management workflows
    - Character list generation and sorting
    - File modification detection and backup behavior
- **Mock Implementation**: Comprehensive mocking patterns established for multiple interfaces:
  - IGamePathService, ILogger<T>, IGameFileService
  - IPlayerCollectionProvider, ITranslationService, ITQDataService, ITagService
  - SessionContext (real instance for complex scenarios)
- **Test Documentation**: AGENTS.md updated with test instructions and examples
- **Coverage Reporting**: Successfully generating coverage reports

### 📊 Current Coverage Status
- **Total Service Tests**: 18 tests created (GameFileService + PlayerService)
- **GameFileService**: ✅ 7 tests passing, 100% test reliability
- **PlayerService**: 🔲 11 tests created, some failing due to complex domain logic
- **Test Execution**: 
  - GameFileService: ~173ms 
  - PlayerService: ~250ms (including test failures)
- **Mock Patterns**: Established for complex service dependencies
- **Test Structure**: Organized under `Services/` folder with proper naming conventions

### 🎯 Next Priority Items
1. **PlayerService Testing**: Apply the same comprehensive approach used for GameFileService
2. **VaultService Testing**: Focus on vault management operations
3. **Domain Entity Testing**: Test core business logic and validation
4. **Integration Testing**: Test service interactions and workflows

## Tools and Technologies

- **Test Framework**: xUnit (already in use)
- **Mocking**: Moq (already included)
- **Coverage**: Coverlet (already included)
- **Assertions**: AwesomeAssertions (already included)
- **CI/CD**: GitHub Actions
- **Coverage Reporting**: Codecov or Coveralls

## Risk Assessment

### High Risk Areas
- **Game File Parsing**: Complex binary formats
- **Legacy Code**: Untested existing functionality
- **GUI Components**: Hard to test Windows Forms

### Mitigation Strategies
- **Incremental Approach**: Test new code first, then legacy
- **Isolation**: Use extensive mocking for complex dependencies
- **Prioritization**: Focus on business-critical paths first

## Recommendations

1. **Start with new development**: Require tests for all new features
2. **Focus on critical paths**: Test most used functionality first
3. **Use TDD for new features**: Test-Driven Development approach
4. **Regular coverage reviews**: Monitor progress weekly
5. **Team training**: Ensure all developers understand testing approach

## Next Steps (Updated)

### Immediate (Next 1-2 weeks)
1. **✅ Test Infrastructure**: Complete (xUnit + Moq + AwesomeAssertions + Coverlet)
2. **PlayerService Testing**: Apply GameFileService patterns to PlayerService
3. **VaultService Testing**: Implement comprehensive vault operation tests
4. **Domain Entity Testing**: Start with core domain entities and validation logic

### Short-term (Next 1-2 months)
1. **Service Layer Completion**: Test all remaining services (PlayerService, VaultService, StashService)
2. **Domain Layer Testing**: Focus on business logic, validation rules, entity behavior
3. **Mock Library Expansion**: Create reusable mock implementations for common interfaces
4. **Test Data Builders**: Implement test data builders for complex domain objects

### Medium-term (Next 2-3 months)
1. **Data Layer Testing**: Repository operations, query methods, error handling
2. **Integration Testing**: Service interactions, workflows, data flows
3. **Utility Class Testing**: Helper methods, extensions, validation utilities
4. **CI/CD Integration**: GitHub Actions workflow with coverage reporting

### Long-term (Next 3-4 months)
1. **GUI/ViewModel Testing**: View model logic, command handling (limited scope)
2. **End-to-End Testing**: Complete user scenarios, system validation
3. **Performance Testing**: Critical path optimization, load testing
4. **Test Maintenance**: Coverage monitoring, test optimization, documentation

## Key Insights from GameFileService Implementation

### 🎯 Successful Patterns
1. **Comprehensive Mocking**: Mock all interface properties to avoid null exceptions
2. **Edge Case Testing**: Test null inputs, disabled features, early returns
3. **Configuration Testing**: Test behavior based on configuration settings
4. **Assertion Library**: AwesomeAssertions provides readable, fluent assertions
5. **Test Organization**: Clear naming conventions and logical test grouping

### 📋 Template for Service Testing
```csharp
public class [ServiceName]Tests
{
    private readonly Mock<ILogger<[ServiceName]>> _mockLogger;
    private readonly Mock<[PrimaryInterface]> _mock[InterfaceName];
    private readonly Mock<[SecondaryInterface]> _mock[InterfaceName];
    private readonly [ServiceName] _service;

    public [ServiceName]Tests()
    {
        // Setup mocks and service instance
    }

    [Fact]
    public void Method_WithCondition_ExpectedResult()
    {
        // Arrange - Setup mocks and test data
        // Act - Call method under test
        // Assert - Verify expected behavior
    }
}
```

### 🔧 Mock Property Checklist
When testing services that use IGamePathService, ensure all these properties are mocked:
- `LocalGitRepositoryDirectory`
- `SaveDirNameTQ`
- `SaveDirNameTQIT`
- `SaveDataDirName`
- `SaveFolderTQ`
- `SaveFolderTQIT`
- `GetVaultList()`
- Any other properties used by the service method

This plan provides a structured approach to systematically increase code coverage while maintaining code quality and ensuring critical functionality is thoroughly tested.