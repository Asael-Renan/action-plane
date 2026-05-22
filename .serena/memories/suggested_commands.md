# Suggested commands

Run these from the repo root on Windows PowerShell unless noted otherwise.

Core .NET commands:
- `dotnet restore`
- `dotnet build`
- `dotnet build -c Release`
- `dotnet test`
- `dotnet test --verbosity detailed`
- `dotnet run --project src/5W2H.App.csproj`
- `dotnet publish -c Release src/5W2H.App.csproj -o publish`
- `dotnet clean`
- `dotnet nuget locals all --clear`

Useful Windows/PowerShell commands:
- `Get-ChildItem` (list files; alias `ls` often works in PowerShell)
- `Get-Content <path>` (print file contents)
- `Set-Location <path>` (change directory; alias `cd` works)
- `Select-String -Path <path> -Pattern <text>` (search text)
- `rg <pattern>` and `rg --files` when ripgrep is available
- `git status`
- `git diff`
- `git log --oneline -n 20`

Typical workflows:
- Restore/build/test: `dotnet restore`, `dotnet build`, `dotnet test`
- Run app: `dotnet run --project src/5W2H.App.csproj`
- Produce distributable build: `dotnet publish -c Release src/5W2H.App.csproj -o publish`

No dedicated formatter or linter config was found in the inspected project files/docs. Standard .NET formatting commands may still be usable if added later.
