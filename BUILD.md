# Build Guide

## Prerequisites

- Windows 10 or 11
- `.NET 8 SDK`

## Development Workflow

Run from the repository root:

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/5W2H.App/5W2H.App.csproj
```

## Release Publish

The WPF project is configured for self-contained `win-x64` release output.

```powershell
dotnet publish -c Release src/5W2H.App/5W2H.App.csproj -o publish
```

Expected output:

- Executable: `publish/5W2H-Management.exe`

## Common Checks

- Build only: `dotnet build`
- Test only: `dotnet test`
- Detailed tests: `dotnet test --verbosity detailed`
- Clean: `dotnet clean`

## Troubleshooting

### `dotnet` not found

Check the SDK installation:

```powershell
dotnet --version
```

### Project path not found

Confirm the WPF project exists:

```powershell
Get-ChildItem src/5W2H.App/5W2H.App.csproj
```

### NuGet restore issues

Clear caches and restore again:

```powershell
dotnet nuget locals all --clear
dotnet restore
```

### WPF app builds but does not start

Run the app project directly in release or debug and inspect the error:

```powershell
dotnet run --project src/5W2H.App/5W2H.App.csproj
```
