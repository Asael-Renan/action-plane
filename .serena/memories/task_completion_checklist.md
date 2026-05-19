# Task completion checklist

After code changes, default verification:
- `dotnet build`
- `dotnet test`
- If UI/runtime behavior changed: `dotnet run --project src/5W2H.App.csproj` when feasible
- If release/distribution behavior changed: `dotnet publish -c Release src/5W2H.App.csproj -o publish`

Before finishing:
- Check XAML bindings and command names against view models when UI changed
- Keep async names with `Async` suffix
- Keep public/core XML docs reasonable when contracts change
- Keep placement consistent: `src/Core`, `src/Data`, `src/UI`, `src/Resources`, `tests`
- Avoid stale docs; confirm against real repo layout

If validation cannot run, report it in handoff.