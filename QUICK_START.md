# Quick Start Guide

## 5 Minute Setup

### 1. Install Prerequisites
- Download and install [.NET 8.0 SDK](https://dotnet.microsoft.com/download)
- Verify: Open PowerShell and type `dotnet --version`

### 2. Clone/Navigate to Project
```bash
cd 5W2H-Management
```

### 3. Build & Run
```bash
# Restore dependencies
dotnet restore

# Run the application
cd src/Presentation.WPF
dotnet run
```

**Application should launch in seconds!**

---

## First Use

When you first run the application:

1. **Database Created**: Automatically created at `%APPDATA%\5W2H-Management\data.db`
2. **Sample Data Loaded**: 3 sample tasks are inserted for demonstration
3. **Main Window Opens**: Shows list of tasks in a DataGrid
4. **Dashboard Available**: View analytics and charts

---

## Main Features to Try

### View Tasks
- The DataGrid displays all tasks automatically
- Click column headers to sort
- Scroll right to see all fields

### Search & Filter
1. Type in "Search" box (filters by What, Why, How, Notes)
2. Set date range with "From" and "To" date pickers
3. Enter responsible person's name
4. Click "Search" button
5. Click "Reset Filters" to see all tasks again

### Example Searches
- Search for "database" → finds "Design Database" task
- Search for "alice" AND set responsible = "Alice" → finds Alice's tasks
- Set date range for last 30 days → shows recent tasks

### Delete Task
1. Click on a task in the DataGrid
2. Click "Delete Selected Task" button
3. Confirm deletion

### View Dashboard (Feature Ready)
Currently in codebase but not linked to main menu.
To use:
1. Open `src/Presentation.WPF/Views/DashboardWindow.xaml.cs`
2. Create instance in MainWindow
3. See summary cards and charts

---

## Understanding the Code Structure

```
You are here: 5W2H-Management/
├── src/
│   ├── Domain/              ← Business logic, entities
│   ├── Application/         ← Use cases, services
│   ├── Infrastructure/      ← Database, repositories
│   └── Presentation.WPF/    ← User interface
└── tests/                   ← Unit tests
```

### Where to Make Changes

| Change Type | Location |
|---|---|
| Add business rule | `src/Domain/Entities/FiveW2HTask.cs` |
| Add search feature | `src/Application/Services/FiveW2HTaskService.cs` |
| Add database query | `src/Infrastructure/Repositories/FiveW2HRepository.cs` |
| Add UI element | `src/Presentation.WPF/Views/MainWindow.xaml` |
| Add UI logic | `src/Presentation.WPF/ViewModels/MainViewModel.cs` |
| Add test | `tests/*/` folder |

---

## Running Tests

```bash
# Run all tests
dotnet test

# Watch for failures
dotnet watch test
```

Tests included:
- ✅ Entity validation tests
- ✅ Service logic tests
- ✅ Export functionality tests

---

## Common Tasks

### Add a New Task (Programmatically)
```csharp
// In MainViewModel.cs or anywhere with IFiveW2HTaskService injected

var service = /* get from DI */;

var newTask = new CreateFiveW2HTaskDto
{
    What = "New project kickoff",
    Why = "Start Q2 initiative",
    Where = "Conference room",
    When = DateTime.UtcNow.AddDays(7),
    Who = "Team Lead",
    How = "Organize meeting agenda",
    HowMuch = 8000,
    Priority = Priority.High,
    Notes = "Prepare slides"
};

var result = await service.CreateTaskAsync(newTask);
```

### Export Data to CSV
```csharp
var tasks = await service.GetAllTasksAsync();
var csv = await exportService.ExportToCsvAsync(tasks);
File.WriteAllText("tasks.csv", csv);
```

### Get Statistics
```csharp
var summary = await service.GetDashboardSummaryAsync();
Console.WriteLine($"Total Tasks: {summary.TotalTasks}");
Console.WriteLine($"Total Cost: ${summary.TotalCost}");
Console.WriteLine($"Completed: {summary.CompletedTasks}");
```

---

## Database Location & Reset

### Find Database File
```
C:\Users\[YourUsername]\AppData\Roaming\5W2H-Management\data.db
```

### Reset Database
Delete the file and restart the application:
```powershell
Remove-Item "$env:APPDATA\5W2H-Management\data.db"
# Now run application again to recreate with sample data
```

### Backup Database
```powershell
Copy-Item "$env:APPDATA\5W2H-Management\data.db" -Destination ".\backup.db"
```

---

## Troubleshooting

### Issue: "Could not find a part of the path"
**Cause**: AppData folder doesn't exist
**Solution**: Create manually: Run → `%APPDATA%` → create folder `5W2H-Management`

### Issue: "The database is locked"
**Cause**: Application still running
**Solution**: Close the application completely and try again

### Issue: "DataGrid shows no columns"
**Cause**: XAML binding issue
**Solution**: Rebuild project: `dotnet clean && dotnet build`

### Issue: "Cannot load WPF assembly"
**Cause**: .NET 8.0 not properly installed
**Solution**: 
```bash
dotnet --version
# Update: https://dotnet.microsoft.com/download
```

---

## Next Steps

### To Learn More
1. Read `ARCHITECTURE.md` for design details
2. Read `BUILD.md` for compilation and distribution
3. Explore tests in `tests/` folder to see patterns
4. Check inline comments in source code

### To Extend Features
1. **Add Create/Edit UI**: Create `AddTaskWindow.xaml` and `EditTaskWindow.xaml`
2. **Add Bulk Operations**: Extend `FiveW2HTaskService` with batch methods
3. **Add Reports**: Create `ReportWindow.xaml` with more analytics
4. **Add Cloud Sync**: Implement new `ICloudSyncService` in Infrastructure
5. **Add Notifications**: Use Win10.Notifications for reminders

### To Deploy
1. Follow steps in `BUILD.md` section "Production Build"
2. Creates single `.exe` file with no dependencies
3. Copy to any Windows machine and run

---

## Key Files Reference

| File | Purpose |
|------|---------|
| `src/Domain/Entities/FiveW2HTask.cs` | Core entity definition |
| `src/Application/Services/FiveW2HTaskService.cs` | Business logic |
| `src/Infrastructure/Repositories/FiveW2HRepository.cs` | Database access |
| `src/Presentation.WPF/ViewModels/MainViewModel.cs` | UI logic |
| `src/Presentation.WPF/Views/MainWindow.xaml` | Main UI layout |
| `tests/Application.Tests/FiveW2HTaskServiceTests.cs` | Example tests |

---

## Getting Help

### Read the Code
- Start with `README.md` for overview
- Read `ARCHITECTURE.md` for design patterns
- Check test files for usage examples
- View comments in source code

### Common Issues
- Check `BUILD.md` troubleshooting section
- Review error messages carefully
- Check Event Viewer for Windows errors

---

**You're ready to go!** 🚀

Start by running the application and exploring the interface, then dig into the code structure.
