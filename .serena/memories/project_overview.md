# 5W2H-Management project overview

Purpose: Windows desktop application for managing 5W2H tasks (What, Why, Where, When, Who, How, How Much) with CRUD flows, filtering, local persistence, import/export, and dashboard charts.

Actual repo shape: a single WPF app project at `src/5W2H.App` plus one xUnit test project at `tests/5W2H.Tests`. Some docs and AGENTS content still describe an older multi-project Clean Architecture layout (`Domain/Application/Infrastructure/Presentation.WPF`), but the current codebase is organized inside one project with logical folders.

Current code structure:
- `src/5W2H.App/Core`: business models and services (`FiveW2HTask`, enums, `TaskService`, `BackupService`, DTOs)
- `src/5W2H.App/Data`: SQLite/Dapper persistence (`AppDbContext`, `TaskRepository`)
- `src/5W2H.App/UI`: WPF MVVM presentation (`Views`, `ViewModels`, `Models`, `Services`, `Converters`)
- `src/5W2H.App/Resources`: theme XAML
- `tests/5W2H.Tests`: unit tests for service and backup logic

Tech stack confirmed from csproj files:
- .NET 8 (`net8.0-windows`)
- WPF
- CommunityToolkit.Mvvm 8.2.2
- Dapper 2.1.15
- System.Data.SQLite 1.0.118.0
- OxyPlot.Wpf 2.1.2
- Microsoft.Extensions.DependencyInjection 8.0.0
- System.Text.Json 8.0.5
- xUnit 2.7.0, Microsoft.NET.Test.Sdk 17.10.0, Moq 4.20.70

Platform notes:
- Windows-only app because it uses WPF
- Release config is self-contained with `RuntimeIdentifier=win-x64`
- Main entrypoint for running is the WPF project `src/5W2H.App/5W2H.App.csproj`
