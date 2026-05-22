# 5W2H-Management project overview

Purpose: Windows desktop application for managing 5W2H tasks (What, Why, Where, When, Who, How, How Much) with CRUD flows, filtering, local persistence, import/export, and dashboard charts.

Actual repo shape: a single WPF app project at `src/5W2H.App.csproj` plus one xUnit test project at `tests/5W2H.Tests.csproj`.

Current code structure:
- `src/Core`: pure business models and rules (`FiveW2HTask`, enums)
- `src/Application`: application services, DTOs, and service contracts (`TaskService`, DTOs, `IBackupService`)
- `src/Data`: SQLite/Dapper persistence (`AppDbContext`, `TaskRepository`)
- `src/Infrastructure`: external implementations for import/export, settings, and updates (`BackupService`, `ThemeService`, `VelopackAppUpdateService`)
- `src/UI`: WPF MVVM presentation (`Views`, `ViewModels`, `Models`, `Services`, `Converters`)
- `src/Resources`: theme XAML
- `tests`: unit tests for service and backup logic

Tech stack confirmed from project configuration:
- .NET 10 (`net10.0-windows`)
- WPF
- CommunityToolkit.Mvvm
- Dapper
- System.Data.SQLite
- OxyPlot.Wpf
- Microsoft.Extensions.DependencyInjection
- System.Text.Json
- xUnit, Moq, Microsoft.NET.Test.Sdk
- Velopack for release packaging

Configuration notes:
- Shared build defaults are centralized in `Directory.Build.props`
- NuGet versions are centralized in `Directory.Packages.props`
- Repo-wide formatting and code style live in `.editorconfig`
- Release config is self-contained for `win-x64`
- Main entrypoint for running is `src/5W2H.App.csproj`
