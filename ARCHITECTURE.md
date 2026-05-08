# Application Architecture

## Overview

This repository is a modular monolithic WPF application organized inside a
single project: `src/5W2H.App`.

The codebase keeps separation by folder and dependency direction, but it is not
a strict multi-project Clean Architecture solution.

## Layers

### Core

Location: `src/5W2H.App/Core`

Purpose:

- Business entities such as `FiveW2HTask`
- Status and priority enums
- Application/business services such as `TaskService` and `BackupService`
- DTOs used by the app

Rules:

- Keep this layer independent from WPF controls and XAML concerns.
- Put validation, workflows, summaries, and backup/export logic here.
- Avoid persistence-specific UI behavior here.

### Data

Location: `src/5W2H.App/Data`

Purpose:

- SQLite initialization through `AppDbContext`
- Dapper-based persistence through `TaskRepository`
- Querying, filtering, and aggregate reads

Rules:

- Keep SQL parameterized.
- Keep persistence details out of view models.
- Do not introduce EF Core unless explicitly requested.

### UI

Location: `src/5W2H.App/UI`

Purpose:

- WPF views and code-behind
- MVVM view models
- Dialog services and file/message abstractions
- Value converters and presentation models

Rules:

- Prefer bindings, commands, and view models over code-behind.
- Keep code-behind limited to view-specific concerns.
- Preserve compatibility with `CommunityToolkit.Mvvm` and `OxyPlot.Wpf`.

### Resources

Location: `src/5W2H.App/Resources`

Purpose:

- Shared styles and theme definitions, especially `ModernTheme.xaml`

## Dependency Direction

Allowed direction:

- `UI` -> `Core`
- `UI` -> `Data`
- `Data` -> `Core`

Avoid:

- `Core` -> `UI`
- WPF-specific types leaking into `Core`
- SQL or repository logic inside view models

## Current Technical Choices

- Runtime: `.NET 8` with `net8.0-windows`
- UI: `WPF`
- MVVM: `CommunityToolkit.Mvvm`
- Persistence: `SQLite` + `Dapper`
- Charts: `OxyPlot.Wpf`
- DI: `Microsoft.Extensions.DependencyInjection`
- Tests: `xUnit` + `Moq`

## Non-Goals

- Reintroducing the old `src/Presentation.WPF` path
- Splitting the app into multiple projects without an explicit architecture task
- Replacing Dapper with another persistence stack by default

## Change Guidance

When modifying the application:

- Put business rules in `Core`.
- Put database/schema/query changes in `Data`.
- Put bindings, commands, dialogs, and screen behavior in `UI`.
- Update tests when service behavior changes.
- Prefer a small number of authoritative docs over many overlapping guides.
