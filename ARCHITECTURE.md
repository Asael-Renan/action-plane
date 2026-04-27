# Clean Architecture Design

## Layer Separation

### Domain Layer (Core Business Logic)
**Location**: `src/Domain/`

The Domain layer contains the heart of the business logic. It is:
- **Framework-agnostic**: No dependencies on EF, Dapper, WPF, or other frameworks
- **Independent**: Knows nothing about the Application, Infrastructure, or Presentation layers
- **Testable**: Can be tested in isolation without mocks

**Key Components**:
```
Entities/
  └── FiveW2HTask          # Core business entity
      ├── Properties (What, Why, Where, When, Who, How, HowMuch, Status, Priority, Notes)
      └── Methods (IsValid())

Enums/
  ├── TaskStatus           # Pending, InProgress, Completed, OnHold, Cancelled
  └── Priority             # Low, Medium, High, Critical

Interfaces/
  ├── IFiveW2HRepository   # Abstraction for data persistence
  ├── IDataExportService   # Abstraction for data export
  └── IDataImportService   # Abstraction for data import
```

**Responsibility**: Define business rules and entities
**Dependencies**: None (Pure C# library)

---

### Application Layer (Business Use Cases)
**Location**: `src/Application/`

The Application layer orchestrates business logic and implements use cases.

**Key Components**:
```
DTOs/                       # Data Transfer Objects
  ├── CreateFiveW2HTaskDto
  ├── UpdateFiveW2HTaskDto
  ├── FiveW2HTaskDto
  └── DashboardSummaryDto

Services/
  ├── FiveW2HTaskService    # CRUD operations, search, dashboard
  └── DataExportService     # CSV/JSON export

Interfaces/
  └── IFiveW2HTaskService   # Service contract
```

**Responsibility**:
- Implement use cases (Create, Read, Update, Delete, Search)
- Orchestrate Domain and Infrastructure layers
- Validate input and business logic
- Provide DTOs for data transfer

**Dependencies**:
- Domain (interfaces and entities)
- System.Text.Json (for JSON export)

---

### Infrastructure Layer (Data Access & External Services)
**Location**: `src/Infrastructure/`

The Infrastructure layer implements the interfaces defined in the Domain layer and handles data persistence.

**Key Components**:
```
Database/
  └── DatabaseInitializer   # SQLite schema creation, migrations, seeding

Repositories/
  └── FiveW2HRepository     # Dapper-based data access

DependencyInjection/
  └── ServiceCollectionExtensions  # DI configuration
```

**Responsibility**:
- Implement IFiveW2HRepository using Dapper and SQLite
- Create and initialize database
- Handle all data persistence concerns
- Configure and register services

**Dependencies**:
- Domain (interfaces)
- Application (services to register)
- Dapper (lightweight ORM)
- System.Data.SQLite (database provider)
- Microsoft.Extensions.DependencyInjection (DI container)

---

### Presentation Layer (User Interface)
**Location**: `src/Presentation.WPF/`

The Presentation layer provides the Windows Presentation Foundation (WPF) user interface.

**Key Components**:
```
Views/
  ├── MainWindow.xaml       # Main task management UI
  ├── MainWindow.xaml.cs    # Code-behind
  ├── DashboardWindow.xaml  # Analytics dashboard
  └── DashboardWindow.xaml.cs

ViewModels/
  ├── MainViewModel         # Main window logic (MVVM)
  └── DashboardViewModel    # Dashboard statistics and charts

Models/
  └── TaskModel             # UI-specific model

Converters/
  ├── BoolToVisibilityConverter
  └── DateTimeToStringConverter

App.xaml                     # Application configuration
App.xaml.cs                  # Startup logic, DI setup
```

**Responsibility**:
- Display user interface
- Handle user interactions
- Bind to ViewModels
- Display data through Data Binding
- Configuration and application startup

**Dependencies**: All layers (uses everything)

---

## Dependency Flow (The Clean Architecture Constraint)

```
                    Presentation.WPF
                          |
                          v
                    Infrastructure
                     /    |    \
                    /     |     \
        Database   Repo   DI    Services
                     \     |     /
                      \    |    /
                          v
                     Application
                        /   \
                      DTOs  Services
                        \   /
                          v
                       Domain
                      Entities
                       Enums
                      Interfaces
```

**Golden Rule**: Code should only reference code in the same layer or inward (toward Domain).
- Presentation references Application, Infrastructure, Domain
- Application references Domain only (not Infrastructure directly)
- Infrastructure references Domain and Application
- Domain references nothing

---

## Data Flow (Typical Operation)

### Example: Search Tasks

```
1. User Interface (View)
   └─> User enters search term and clicks "Search"

2. View binding triggers ViewModel (MainViewModel)
   └─> SearchTasksCommand executes

3. ViewModel calls Application Service (IFiveW2HTaskService)
   └─> taskService.SearchTasksAsync(searchText, dates, responsible)

4. Application Service (FiveW2HTaskService)
   └─> Validates input
   └─> Calls Domain Repository Interface (IFiveW2HRepository)

5. Infrastructure Repository (FiveW2HRepository)
   └─> Builds SQL query with Dapper
   └─> Executes against SQLite database
   └─> Returns Domain Entities (FiveW2HTask)

6. Application Service maps to DTO
   └─> Returns FiveW2HTaskDto

7. ViewModel converts to UI Model
   └─> Updates ObservableCollection<TaskModel>

8. View (XAML Binding)
   └─> DataGrid displays results
```

---

## SOLID Principles in Practice

### 1. Single Responsibility Principle
```csharp
// ❌ BAD: Service doing too much
public class FiveW2HService
{
    public void CreateTask() { }      // Domain logic
    public void SaveToDatabase() { }  // Data access
    public void ExportToCSV() { }     // Export logic
    public void UpdateUI() { }        // UI logic
}

// ✅ GOOD: Separated concerns
FiveW2HTaskService       // Handles task operations
FiveW2HRepository        // Handles data persistence
DataExportService        // Handles export
MainViewModel            // Handles UI
```

### 2. Open/Closed Principle
```csharp
// ✅ Open for extension, closed for modification
// Interface allows new implementations without changing existing code
public interface IFiveW2HRepository
{
    Task<FiveW2HTask?> GetByIdAsync(int id);
    // ... other methods
}

// Can add new implementations without modifying existing ones
public class FiveW2HRepository : IFiveW2HRepository { }
public class MockFiveW2HRepository : IFiveW2HRepository { }  // For testing
```

### 3. Liskov Substitution Principle
```csharp
// ✅ Implementations can be substituted for interface
IFiveW2HRepository repo = new FiveW2HRepository(connectionString);
// or for testing:
IFiveW2HRepository mockRepo = new Mock<IFiveW2HRepository>();
// Both work the same from caller's perspective
```

### 4. Interface Segregation Principle
```csharp
// ✅ Small, focused interfaces instead of fat interfaces
public interface IFiveW2HTaskService
{
    Task<FiveW2HTaskDto?> GetTaskAsync(int id);
    Task<FiveW2HTaskDto> CreateTaskAsync(CreateFiveW2HTaskDto dto);
    Task<FiveW2HTaskDto> UpdateTaskAsync(UpdateFiveW2HTaskDto dto);
    Task<bool> DeleteTaskAsync(int id);
}

// Not: public interface IService { ... 50 methods ... }
```

### 5. Dependency Inversion Principle
```csharp
// ✅ Depend on abstractions, not concrete implementations
public class FiveW2HTaskService : IFiveW2HTaskService
{
    private readonly IFiveW2HRepository _repository;  // Abstraction
    
    public FiveW2HTaskService(IFiveW2HRepository repository)
    {
        _repository = repository;
    }
}

// Configured via DI Container
services.AddScoped<IFiveW2HRepository, FiveW2HRepository>();
services.AddScoped<IFiveW2HTaskService, FiveW2HTaskService>();
```

---

## Testing Strategy

### Unit Tests by Layer

#### Domain Tests (`tests/Domain.Tests/`)
```csharp
// Test business entities and rules
[Fact]
public void FiveW2HTask_IsValid_WithValidData_ReturnsTrue()
{
    var task = new FiveW2HTask { What = "x", Why = "y", Who = "z", How = "w", HowMuch = 0 };
    Assert.True(task.IsValid());
}
```

#### Application Tests (`tests/Application.Tests/`)
```csharp
// Test service logic with mocked repository
[Fact]
public async Task CreateTaskAsync_WithValidDto_ReturnsTaskDto()
{
    var mockRepo = new Mock<IFiveW2HRepository>();
    mockRepo.Setup(r => r.AddAsync(It.IsAny<FiveW2HTask>())).ReturnsAsync(1);
    
    var service = new FiveW2HTaskService(mockRepo.Object);
    var result = await service.CreateTaskAsync(dto);
    
    Assert.NotNull(result);
    mockRepo.Verify(r => r.AddAsync(It.IsAny<FiveW2HTask>()), Times.Once);
}
```

#### Infrastructure Tests (`tests/Infrastructure.Tests/`)
```csharp
// Test data export without mocking
[Fact]
public async Task ExportToCsvAsync_WithTasks_ReturnsCsvString()
{
    var service = new DataExportService();
    var csv = await service.ExportToCsvAsync(tasks);
    Assert.NotEmpty(csv);
}
```

---

## Database Design

### FiveW2HTasks Table

```sql
CREATE TABLE FiveW2HTasks (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    What TEXT NOT NULL,
    Why TEXT NOT NULL,
    Where TEXT,
    When DATETIME NOT NULL,
    Who TEXT NOT NULL,
    How TEXT NOT NULL,
    HowMuch DECIMAL(10, 2) NOT NULL,
    Status INTEGER NOT NULL DEFAULT 0,  -- 0:Pending, 1:InProgress, 2:Completed, 3:OnHold, 4:Cancelled
    Priority INTEGER NOT NULL DEFAULT 2,  -- 1:Low, 2:Medium, 3:High, 4:Critical
    Notes TEXT,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for query performance
CREATE INDEX IDX_FiveW2HTasks_Status ON FiveW2HTasks(Status);
CREATE INDEX IDX_FiveW2HTasks_Who ON FiveW2HTasks(Who);
CREATE INDEX IDX_FiveW2HTasks_When ON FiveW2HTasks(When);
CREATE INDEX IDX_FiveW2HTasks_Priority ON FiveW2HTasks(Priority);
```

---

## Extending the Architecture

### Adding a New Feature: Task Templates

1. **Domain Layer** - Define the entity and interface:
   ```csharp
   // src/Domain/Entities/TaskTemplate.cs
   public class TaskTemplate { public string Name { get; set; } /* ... */ }
   
   // src/Domain/Interfaces/ITaskTemplateRepository.cs
   public interface ITaskTemplateRepository { /* ... */ }
   ```

2. **Application Layer** - Create service:
   ```csharp
   // src/Application/Services/TaskTemplateService.cs
   public class TaskTemplateService : ITaskTemplateService { /* ... */ }
   ```

3. **Infrastructure Layer** - Implement repository:
   ```csharp
   // src/Infrastructure/Repositories/TaskTemplateRepository.cs
   public class TaskTemplateRepository : ITaskTemplateRepository { /* ... */ }
   ```

4. **Presentation Layer** - Create UI:
   ```csharp
   // src/Presentation.WPF/Views/TemplatesWindow.xaml
   // src/Presentation.WPF/ViewModels/TemplatesViewModel.cs
   ```

5. **Tests** - Add tests for each layer

---

## Performance Optimization Tips

1. **Use Dapper Projections**: Load only needed columns
   ```csharp
   var tasks = await connection.QueryAsync<(int Id, string What)>(
       "SELECT Id, What FROM FiveW2HTasks WHERE Status = @Status",
       new { Status = (int)TaskStatus.Pending }
   );
   ```

2. **Batch Operations**: Insert multiple tasks at once
   ```csharp
   await connection.ExecuteAsync(sql, tasks);  // Dapper batches automatically
   ```

3. **Pagination**: Load data in chunks
   ```csharp
   const int pageSize = 50;
   var offset = (pageNumber - 1) * pageSize;
   ```

4. **Async/Await**: Prevent blocking the UI thread
   ```csharp
   await taskService.GetAllTasksAsync();
   ```

---

## Conclusion

This architecture provides:
- ✅ Clear separation of concerns
- ✅ Testability at every layer
- ✅ Maintainability and extensibility
- ✅ SOLID principles adherence
- ✅ Independent frameworks
- ✅ Production-ready code quality

By following these patterns, the codebase remains flexible, testable, and ready for new requirements.
