# 5W2H-Management project overview

Purpose: Windows desktop application for managing 5W2H tasks (What, Why, Where, When, Who, How, How Much) with CRUD flows, filtering, local persistence, import/export, and dashboard charts.

Actual repo shape: a single WPF app project at `src/5W2H.App` plus one xUnit test project at `tests/5W2H.Tests`.

Current code structure:
- `src/5W2H.App/Core`: business models and services (`FiveW2HTask`, enums, `TaskService`, `BackupService`, DTOs)
- `src/5W2H.App/Data`: SQLite/Dapper persistence (`AppDbContext`, `TaskRepository`)
- `src/5W2H.App/UI`: WPF MVVM presentation (`Views`, `ViewModels`, `Models`, `Services`, `Converters`)
- `src/5W2H.App/Resources`: theme XAML
- `tests/5W2H.Tests`: unit tests for service and backup logic

Tech stack confirmed from project configuration:
- .NET 8 (`net8.0-windows`)
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
- Main entrypoint for running is `src/5W2H.App/5W2H.App.csproj`
