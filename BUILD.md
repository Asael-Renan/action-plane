# Build & Publish Guide

## Prerequisites

- .NET 8.0 SDK or later (download from [dotnet.microsoft.com](https://dotnet.microsoft.com))
- Visual Studio 2022 or VS Code
- Windows 10/11 (WPF is Windows-only)

## Development Build

### 1. Clone/Open Project
```bash
cd 5W2H-Management
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build Solution
```bash
# Debug build
dotnet build

# Release build
dotnet build -c Release
```

### 4. Run Tests
```bash
# All tests
dotnet test

# Specific test project
dotnet test tests/Domain.Tests/Domain.Tests.csproj
dotnet test tests/Application.Tests/Application.Tests.csproj
dotnet test tests/Infrastructure.Tests/Infrastructure.Tests.csproj

# With verbose output
dotnet test --verbosity detailed
```

### 5. Run Application
```bash
cd src/Presentation.WPF
dotnet run
```

## Production Build (Self-Contained Executable)

### Option 1: Using dotnet publish (Recommended)

```bash
# Create self-contained, single-file executable
cd 5W2H-Management
dotnet publish -c Release -o publish

# The executable is at: publish/5W2H-Management.exe
```

#### Configuration Details
The project file includes these settings for standalone deployment:
```xml
<PublishReadyToRun>true</PublishReadyToRun>
<SelfContained>true</SelfContained>
<RuntimeIdentifier>win-x64</RuntimeIdentifier>
```

### Option 2: Manual Steps

```bash
# Step 1: Clean previous builds
dotnet clean -c Release

# Step 2: Build in Release mode
dotnet build -c Release

# Step 3: Publish to folder
dotnet publish -c Release --self-contained -o dist/5W2H-Management

# Step 4: Executable is at:
# dist/5W2H-Management/5W2H-Management.exe
```

## Output & Files

### Debug Build Output
```
bin/Debug/net8.0-windows/
├── 5W2H-Management.exe
├── 5W2H-Management.dll
└── [dependencies]
```

### Release Build Output
```
publish/
├── 5W2H-Management.exe          # Main executable (self-contained)
└── [embedded dependencies - not visible]
```

**File Size**: ~80-100 MB (includes .NET runtime)

## Distribution

### Create Installer (Optional)

You can wrap the executable in an installer using:
- **WiX Toolset**: Free, open-source MSI creator
- **NSIS**: Nullsoft Scriptable Install System
- **Advanced Installer**: Commercial solution
- **InnoSetup**: Free, easy to use

### Simple Distribution Steps

1. **Create Release Folder**:
   ```bash
   mkdir releases/v1.0
   copy publish/5W2H-Management.exe releases/v1.0/
   ```

2. **Create README.txt**:
   ```
   5W2H Task Management System v1.0
   
   Requirements:
   - Windows 10/11 (64-bit)
   - No additional installation needed
   
   Installation:
   - Extract 5W2H-Management.exe
   - Double-click to run
   
   Data:
   - Database stored at: C:\Users\[YourName]\AppData\Roaming\5W2H-Management\
   
   Support:
   - [Your contact info]
   ```

3. **Zip for Distribution**:
   ```bash
   Compress-Archive -Path releases/v1.0/* -DestinationPath 5W2H-Management-v1.0.zip
   ```

## Deployment Scenarios

### Scenario 1: Single User on Local Machine
1. Run `dotnet publish -c Release -o publish`
2. Copy `publish/5W2H-Management.exe` to desired location
3. Create shortcut on Desktop (right-click → Send to → Desktop)
4. Done! Run anytime by double-clicking executable

### Scenario 2: Shared Network Drive
```bash
# Publish to network location
dotnet publish -c Release -o \\network\shared\5W2H-Management

# Users access via:
\\network\shared\5W2H-Management\5W2H-Management.exe
```

### Scenario 3: Silent Installation (Scripted)
```powershell
# PowerShell script to deploy to multiple machines
$exe = "5W2H-Management.exe"
$targetPath = "C:\Program Files\5W2H-Management"

# Create folder
New-Item -ItemType Directory -Path $targetPath -Force

# Copy executable
Copy-Item -Path $exe -Destination $targetPath

# Create shortcut
$WshShell = New-Object -ComObject WScript.Shell
$Shortcut = $WshShell.CreateShortcut("$env:USERPROFILE\Desktop\5W2H Management.lnk")
$Shortcut.TargetPath = "$targetPath\5W2H-Management.exe"
$Shortcut.Save()

Write-Host "Installation complete!"
```

## Troubleshooting Build Issues

### Issue: "Error: Unable to find runtime"
```
Solution: Ensure .NET 8.0 SDK is installed
dotnet --version
# Should show 8.0.x or higher
```

### Issue: "Project references..."
```
Solution: Clean and restore
dotnet clean
dotnet restore
```

### Issue: "NuGet package not found"
```
Solution: Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore
```

### Issue: "Cannot open WPF designer"
```
Solution: This is normal in some environments
- Build project instead: dotnet build
- Use WPF designer in Visual Studio if available
```

## Runtime Requirements

### System Requirements
- **OS**: Windows 10 version 1909+ or Windows 11
- **Architecture**: x64 (64-bit)
- **RAM**: Minimum 512 MB, Recommended 2 GB
- **Disk**: ~150 MB for executable + database

### No External Dependencies
- Self-contained executable includes .NET 8.0 runtime
- SQLite is bundled
- No registry modifications
- Safe to delete folder = uninstall

## Updating the Application

### To Update to a New Version:

1. **Backup Data** (optional):
   ```bash
   copy %APPDATA%\5W2H-Management\data.db backup.db
   ```

2. **Delete Old Executable**:
   ```bash
   del "C:\Program Files\5W2H-Management\5W2H-Management.exe"
   ```

3. **Copy New Executable**:
   ```bash
   copy new\5W2H-Management.exe "C:\Program Files\5W2H-Management\"
   ```

Database migrations are handled automatically on startup.

## Build Customization

### Change Application Name
Edit `src/Presentation.WPF/Presentation.WPF.csproj`:
```xml
<AssemblyName>YourAppName</AssemblyName>
```

### Change Icon
1. Create 256x256 PNG icon
2. Convert to .ico using online tool or Visual Studio
3. Place in `src/Presentation.WPF/`
4. Add to .csproj:
```xml
<ApplicationIcon>icon.ico</ApplicationIcon>
```

### Change Version
Edit `src/Presentation.WPF/Presentation.WPF.csproj`:
```xml
<Version>1.0.0</Version>
<AssemblyVersion>1.0.0.0</AssemblyVersion>
<FileVersion>1.0.0.0</FileVersion>
```

## Performance Optimization

### For Release Build
```bash
# Add trimming (removes unused code - advanced)
dotnet publish -c Release -p:PublishTrimmed=true -o publish

# Add ReadyToRun (pre-JIT compilation)
dotnet publish -c Release -p:PublishReadyToRun=true -o publish

# All optimizations
dotnet publish -c Release \
  -p:PublishTrimmed=true \
  -p:PublishReadyToRun=true \
  --self-contained \
  -o publish
```

**Note**: Trimming can be aggressive; test thoroughly

## Continuous Integration (CI/CD)

### GitHub Actions Example
```yaml
name: Build and Publish
on: [push]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v3
      - uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0'
      - run: dotnet restore
      - run: dotnet build -c Release
      - run: dotnet test
      - run: dotnet publish -c Release -o publish
      - uses: actions/upload-artifact@v3
        with:
          name: 5W2H-Management
          path: publish/5W2H-Management.exe
```

## Signing the Executable (Code Signing)

For production releases, sign the executable to show it's trusted:

```bash
# Using signtool (from Windows SDK)
signtool sign /f "certificate.pfx" /p password /t "http://timestamp.authority.com" publish\5W2H-Management.exe
```

This prevents Windows SmartScreen warnings on Windows 11.

## Cleanup After Build

```bash
# Remove temporary build files
dotnet clean

# Remove publish folder
rmdir /s /q publish

# Clear NuGet cache if needed
dotnet nuget locals all --clear
```

---

**Status**: Ready for Production Distribution ✅
