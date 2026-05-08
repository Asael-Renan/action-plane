# Task completion checklist

After code changes, the default verification path for this repo is:
- `dotnet build`
- `dotnet test`
- If UI/runtime behavior changed, run `dotnet run --project src/5W2H.App/5W2H.App.csproj` when feasible
- If release/distribution behavior changed, run `dotnet publish -c Release src/5W2H.App/5W2H.App.csproj -o publish`

When finishing a task, also check:
- WPF bindings/command names remain consistent between XAML and view models
- New async methods follow the existing `Async` naming convention
- XML docs remain reasonable for public interfaces/core service contracts
- Architectural placement is consistent with current folders: `Core`, `Data`, `UI`, `Resources`
- Avoid relying on stale doc assumptions; confirm changes against the real `src/5W2H.App` layout

If tests or runtime validation cannot be executed, explicitly report that in the handoff.
