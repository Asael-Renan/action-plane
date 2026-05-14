# 5W2H Management

Windows desktop application for managing 5W2H actions with local persistence,
search/filtering, backup/export flows, and dashboard charts.

## What This Repo Is

- WPF application targeting `.NET 8` on Windows
- Modular monolith inside `src/5W2H.App`
- Local persistence with `SQLite` and `Dapper`
- MVVM UI built with `CommunityToolkit.Mvvm`
- Test project in `tests/5W2H.Tests`

## Repository Shape

```text
5W2H-Management/
|-- src/
|   `-- 5W2H.App/
|       |-- Core/
|       |-- Data/
|       |-- Resources/
|       `-- UI/
`-- tests/
    `-- 5W2H.Tests/
```

## Main Commands

From the repository root:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/5W2H.App/5W2H.App.csproj
dotnet publish -c Release -r win-x64 --self-contained true src/5W2H.App/5W2H.App.csproj -o publish
```

## Project Notes

- The app is Windows-only because it uses WPF.
- Release builds are self-contained for `win-x64`.
- Installer/update releases use Velopack assets published to GitHub Releases.
- The database is created automatically on first run under `%APPDATA%`.
- The architecture is intentionally a single application project, not the
  older split `Domain/Application/Infrastructure/Presentation.WPF` layout.

## Documentation

- [AGENTS.md](AGENTS.md): operational guidance for AI agents
- [ARCHITECTURE.md](ARCHITECTURE.md): architectural boundaries and dependency rules
- [BUILD.md](BUILD.md): build, test, and publish workflow
- [DECISIONS.md](DECISIONS.md): durable technical decisions for this repository
