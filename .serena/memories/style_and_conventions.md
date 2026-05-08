# Style and conventions

Observed code conventions from the current source:
- C# with `Nullable` enabled and `ImplicitUsings` enabled
- `LangVersion` is `latest`
- Namespaces follow the root namespace `_5W2H.App`
- Public members and interfaces commonly use XML documentation comments, especially in core models/services
- Async methods use `Async` suffix (`GetTaskAsync`, `CreateTaskAsync`, `SearchTasksAsync`)
- Constructor injection is used for services and view models, with null checks via `ArgumentNullException`
- MVVM uses CommunityToolkit attributes such as `[ObservableProperty]` and `[RelayCommand]`
- View models are `partial` classes inheriting from `ObservableObject`
- Private fields use leading underscore (`_taskService`)
- Public types/members use PascalCase; private backing fields generated through toolkit stay lower camel case in attributed fields
- String properties are commonly initialized to `string.Empty`
- UI logic is kept in `UI/ViewModels`; persistence is in `Data`; business/domain logic is in `Core`

Project-specific guidance:
- Prefer preserving the current single-project folder architecture rather than introducing the older documented multi-project layout unless the user explicitly asks for a broader refactor
- Keep WPF/MVVM patterns intact: bindings in XAML, commands/properties in view models, service abstractions for dialogs and data access
- When docs conflict with the actual repo, trust the actual source tree and csproj files first
