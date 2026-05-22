# Style and conventions

Observed and now codified conventions from the current source/config:
- Shared .NET defaults live in `Directory.Build.props`
- Package versions are centralized in `Directory.Packages.props`
- Formatting/style defaults live in `.editorconfig`
- C# with `Nullable` enabled, `ImplicitUsings` enabled, `LangVersion=latest`
- `EnableNETAnalyzers=true`, `AnalysisLevel=latest-recommended`, `Deterministic=true`
- Namespaces follow the root namespace `FiveW2H.App`
- Public members and interfaces commonly use XML documentation comments, especially in core models/services
- Async methods use `Async` suffix (`GetTaskAsync`, `CreateTaskAsync`, `SearchTasksAsync`)
- Constructor injection is used for services and view models, with null checks via `ArgumentNullException`
- MVVM uses CommunityToolkit attributes such as `[ObservableProperty]` and `[RelayCommand]`
- View models are `partial` classes inheriting from `ObservableObject`
- Private fields use leading underscore (`_taskService`)
- String properties are commonly initialized to `string.Empty`
- `.editorconfig` preferences: spaces, LF by default, CRLF only for `.sln`, C# indent size 4, `var` discouraged by default
- UI logic stays in `UI/ViewModels`; persistence stays in `Data`; pure business rules stay in `Core`; application use cases and DTOs stay in `Application`; external integrations stay in `Infrastructure`

Project-specific guidance:
- Preserve the current single-project folder architecture (`Core`, `Application`, `Data`, `Infrastructure`, `UI`) rather than reintroducing the older multi-project layout unless explicitly requested
- Keep WPF/MVVM patterns intact: bindings in XAML, commands/properties in view models, service abstractions for dialogs and data access
- When docs conflict with the actual repo, trust the source tree and project files first
