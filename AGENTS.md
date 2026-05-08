# AGENTS.md - 5W2H Management

Authoritative instructions for coding agents working in this repository.

This project is a Windows-only WPF desktop application for managing 5W2H
actions with CRUD flows, filters, dashboard charts, SQLite persistence, and
backup/import/export support.

## Source Of Truth

Use the current source tree and project files as the primary source of truth.
Documentation is useful, but source code wins when docs and code disagree.

Important historical context from SERENA project memory and recent git history:

- The repository was simplified from an older multi-project Clean Architecture
  layout into a single WPF application project.
- Recent commits include `dcb84f1 simplificando arquitetura` and
  `2cd4243 simplificando arquitetura`.
- Do not recreate the old `src/Domain`, `src/Application`,
  `src/Infrastructure`, or `src/Presentation.WPF` structure unless the user
  explicitly asks for a larger architecture migration.

## Required SERENA Workflow

SERENA MCP is required for non-trivial work in this repository.
If SERENA is unavailable, state that limitation clearly before proceeding with
local-only context.

Before substantial analysis or edits:

1. Activate/check the project with SERENA.
2. Check onboarding status.
3. Read relevant memories first:
   - `project_overview`
   - `style_and_conventions`
   - `suggested_commands`
   - `task_completion_checklist`
4. Prefer SERENA symbolic tools for C# exploration:
   - `get_symbols_overview` for unfamiliar files.
   - `find_symbol` for classes, interfaces, methods, and properties.
   - `find_referencing_symbols` before changing public contracts.
   - `search_for_pattern` for XAML, Markdown, config, or uncertain names.
5. Read whole source files only when symbolic/contextual reads are insufficient.

Update SERENA memories only when a task changes durable project facts such as
architecture, commands, conventions, or completion checklist.

## Current Repository Shape

```text
5W2H-Management/
|-- 5W2H-Management.sln
|-- src/
|   `-- 5W2H.App/
|       |-- 5W2H.App.csproj
|       |-- App.xaml
|       |-- App.xaml.cs
|       |-- Core/
|       |   |-- Models/
|       |   `-- Services/
|       |-- Data/
|       |-- Resources/
|       `-- UI/
|           |-- Converters/
|           |-- Models/
|           |-- Services/
|           |-- ViewModels/
|           `-- Views/
`-- tests/
    `-- 5W2H.Tests/
```

## Technology Stack

- .NET 8, `net8.0-windows`
- WPF
- CommunityToolkit.Mvvm
- SQLite via `System.Data.SQLite`
- Dapper
- OxyPlot.Wpf for dashboard charts
- Microsoft.Extensions.DependencyInjection
- System.Text.Json
- xUnit, Moq, Microsoft.NET.Test.Sdk

Release builds are self-contained for `win-x64`.

## Architectural Rules

The current architecture is a modular monolith inside `src/5W2H.App`.
Maintain pragmatic separation by folder, but do not treat the repository as a
strict multi-project Clean Architecture implementation.

- `Core/Models`: business entities and enums. Keep this UI-independent.
- `Core/Services`: business/application services and DTOs. Keep workflows,
  validation, summaries, backup/export logic, and service contracts here.
- `Data`: SQLite/Dapper persistence. Keep SQL, database initialization, and
  repository implementations here.
- `UI/ViewModels`: MVVM state and commands. Use CommunityToolkit.Mvvm
  attributes and keep direct UI control manipulation out of view models.
- `UI/Views`: XAML and code-behind. Keep code-behind minimal and focused on
  view concerns that are hard to express through binding.
- `UI/Services`: dialog/file/message abstractions and implementations.
- `UI/Converters`: value converters only.
- `Resources/ModernTheme.xaml`: shared visual styling and theme resources.
- `tests/5W2H.Tests`: isolated xUnit coverage for services and behavior.

Do not introduce Entity Framework unless explicitly requested; persistence is
currently SQLite plus Dapper.

Do not introduce new UI frameworks or charting libraries unless there is a
clear reason; the app already uses WPF, MVVM, and OxyPlot.

## Coding Conventions

- Use namespace root `_5W2H.App`.
- Keep nullable reference types enabled and code null-safe.
- Use `Async` suffix for asynchronous methods.
- Use constructor injection for services and view models.
- Use `ArgumentNullException` guards for injected dependencies.
- Use leading underscores for private fields.
- Use `string.Empty` for default string values.
- Keep public types, interfaces, and important public members documented with
  concise XML comments when they are part of application/core contracts.
- Preserve the existing pattern of partial view models inheriting from
  `ObservableObject`.
- Prefer `[ObservableProperty]` and `[RelayCommand]` over hand-written
  boilerplate unless the existing code requires a custom implementation.
- Keep internal code names in English. Preserve existing Portuguese
  user-facing labels and messages unless the user requests a language change.

## Agent Routing

Use these scopes to decide which specialist behavior applies.

### Project Architect

Applies to:

- `AGENTS.md`
- `ARCHITECTURE.md`
- `BUILD.md`
- `DECISIONS.md`
- `README.md`
- `*.sln`
- `*.csproj`

Responsibilities:

- Keep docs aligned with the real single-project architecture.
- Prevent accidental reintroduction of stale multi-project paths.
- Confirm dependency and packaging changes against the `.csproj` files.
- Document major architecture changes in `ARCHITECTURE.md` when code structure
  materially changes.

### Core Services Developer

Applies to:

- `src/5W2H.App/Core/**/*.cs`

Responsibilities:

- Maintain `FiveW2HTask`, 5W2H fields, status/priority enums, validation, DTOs,
  task workflows, backup/export behavior, and dashboard summaries.
- Keep core code independent from WPF controls and visual concerns.
- Update or add tests when business rules change.

Important current symbols:

- `FiveW2HTask`
- `ITaskService`
- `TaskService`
- `BackupService`

### Data Persistence Developer

Applies to:

- `src/5W2H.App/Data/**/*.cs`

Responsibilities:

- Maintain `AppDbContext`, schema initialization, SQLite access, and Dapper
  repository queries.
- Keep SQL parameterized.
- Keep persistence details out of view models.
- Verify repository contract changes against service usage and tests.

Important current symbols:

- `ITaskRepository`
- `TaskRepository`
- `AppDbContext`

### WPF MVVM Developer

Applies to:

- `src/5W2H.App/UI/**/*.xaml`
- `src/5W2H.App/UI/**/*.xaml.cs`
- `src/5W2H.App/UI/ViewModels/**/*.cs`
- `src/5W2H.App/UI/Views/**/*.cs`
- `src/5W2H.App/UI/Converters/**/*.cs`
- `src/5W2H.App/UI/Services/**/*.cs`
- `src/5W2H.App/Resources/**/*.xaml`

Responsibilities:

- Keep bindings, generated command names, and view model properties consistent.
- Use MVVM first; avoid business logic in XAML code-behind.
- Preserve the existing visual direction in `ModernTheme.xaml`.
- Keep dashboards compatible with OxyPlot models.
- Check both desktop usability and smaller-window layout behavior when changing
  XAML.

Important current symbols/files:

- `MainViewModel`
- `EditItemViewModel`
- `DashboardViewModel`
- `MainWindow.xaml`
- `EditItemWindow.xaml`
- `DashboardWindow.xaml`
- `ModernTheme.xaml`

### Test Engineer

Applies to:

- `tests/**/*.cs`
- `**/*Tests.cs`

Responsibilities:

- Use xUnit and AAA structure.
- Prefer isolated tests around services and behavior.
- Use Moq for external dependencies when useful.
- Add regression tests for fixed bugs.
- Keep tests deterministic and independent from local machine state.

Current test areas:

- `TaskServiceTests`
- `BackupServiceTests`

## Change Discipline

- Check the worktree before editing; the repo may contain user changes.
- Do not revert unrelated user edits.
- Keep changes scoped to the requested task.
- For public contract changes, use SERENA references to update call sites.
- Prefer small, direct changes over broad rewrites.
- If source and docs disagree, update docs only when the task requires it or
  the change creates a new durable fact.
- For Markdown-only changes, do not run full application validation unless
  requested or unless the docs change commands/configuration that should be
  verified.

## Verification

Run the narrowest useful verification for the change.

Common commands from the repository root:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/5W2H.App/5W2H.App.csproj
dotnet publish -c Release src/5W2H.App/5W2H.App.csproj -o publish
```

Default verification expectations:

- C# changes: run `dotnet build` and `dotnet test`.
- XAML/UI changes: run `dotnet build`; run the app when feasible.
- Test-only changes: run `dotnet test`.
- Packaging/release changes: run
  `dotnet publish -c Release src/5W2H.App/5W2H.App.csproj -o publish`.
- Documentation-only changes: review links, paths, commands, and architecture
  statements for consistency.

If verification cannot be run, report that explicitly in the handoff.

## Do Not

- Do not use stale paths such as `src/Presentation.WPF`.
- Do not split the app into multiple projects without explicit approval.
- Do not add EF Core to solve persistence tasks; use the existing Dapper
  repository pattern.
- Do not put database code in view models.
- Do not put WPF control logic in core services.
- Do not rename generated MVVM properties or commands without checking XAML
  bindings.
- Do not change Portuguese UI text unless requested.
