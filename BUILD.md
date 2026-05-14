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

The WPF project is configured for self-contained `win-x64` release output and
Velopack packaging. GitHub Releases is the update source used by the app.

```powershell
dotnet publish -c Release -r win-x64 --self-contained true src/5W2H.App/5W2H.App.csproj -o publish
```

Expected output:

- Executable: `publish/5W2H-Management.exe`

## Installer and Updates

The installed app checks GitHub Releases at:

```text
https://github.com/Asael-Renan/action-plane
```

Manual local packaging requires the Velopack CLI version that matches the
application package reference:

```powershell
dotnet tool install -g vpk --version 0.0.1298
vpk download github --repoUrl https://github.com/Asael-Renan/action-plane
vpk pack --packId 5W2H-Management --packVersion 1.0.0 --packDir publish --mainExe 5W2H-Management.exe --outputDir Releases --packTitle "5W2H Management" --packAuthors "Asael Renan" --releaseNotes RELEASE_NOTES.md
vpk upload github --repoUrl https://github.com/Asael-Renan/action-plane --outputDir Releases --publish --releaseName "5W2H Management 1.0.0" --tag v1.0.0
```

The preferred release path is the manual GitHub Actions workflow in
`.github/workflows/release.yml`. Run it with the SemVer version to publish,
for example `1.0.1`.

Before each release:

- Update `<Version>` in `src/5W2H.App/5W2H.App.csproj`.
- Update `RELEASE_NOTES.md`.
- Run the release workflow with the same version.

Client update behavior:

- First install uses `Setup.exe` from the latest GitHub Release.
- Later updates are detected from inside the app through the `Atualizacoes`
  sidebar command and the startup check.
- Local SQLite data stays under `%APPDATA%/5W2H-Management/` and is not inside
  the installed app directory.

Current limitation: the installer and binaries are not code-signed. Windows may
show SmartScreen or publisher warnings until code signing is added.

If the GitHub repository is private, automatic updates from client machines will
not work without an authenticated update source. Use public release assets or
move the Velopack packages to an HTTPS host accessible to the client.

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
