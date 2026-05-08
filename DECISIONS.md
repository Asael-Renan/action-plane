# Technical Decisions

This file records durable decisions that AI agents and maintainers should
treat as repository defaults unless a task explicitly changes them.

## Current Decisions

### Single-project architecture

The application lives in one WPF project: `src/5W2H.App`.

Implication:

- Do not reintroduce `src/Domain`, `src/Application`,
  `src/Infrastructure`, or `src/Presentation.WPF` as the default structure.

### Modular monolith

The project separates concerns by folders, not by independent runtime projects.

Implication:

- Keep boundaries clear, but do not force a heavier architecture than the code
  currently needs.

### Persistence stack

Persistence uses `SQLite` with `Dapper`.

Implication:

- Prefer extending `AppDbContext` and `TaskRepository` over introducing EF Core
  or another ORM by default.

### UI stack

The UI uses `WPF` with `CommunityToolkit.Mvvm`.

Implication:

- Keep business logic out of code-behind.
- Prefer bindings, observable properties, and relay commands.

### Charting stack

Dashboards use `OxyPlot.Wpf`.

Implication:

- Preserve compatibility with existing plot model usage unless a charting
  replacement is explicitly requested.

### Test strategy

Tests live in `tests/5W2H.Tests` and focus on services and behavior.

Implication:

- Prefer unit tests for business/service changes.
- Add regression tests for bug fixes that affect domain logic or workflows.
