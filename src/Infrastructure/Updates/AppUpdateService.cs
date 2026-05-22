using System.Reflection;
using Velopack;
using Velopack.Sources;

namespace FiveW2H.App.Infrastructure.Updates;

public interface IAppUpdateService
{
    Task<AppUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default);
    Task DownloadAndApplyUpdateAsync(CancellationToken cancellationToken = default);
}

public sealed record AppUpdateCheckResult(
    bool IsInstalled,
    bool IsUpdateAvailable,
    string CurrentVersion,
    string? AvailableVersion,
    string Message);

public sealed class VelopackAppUpdateService : IAppUpdateService
{
    private const string RepositoryUrl = "https://github.com/Asael-Renan/action-plane";

    private readonly UpdateManager _updateManager;
    private UpdateInfo? _availableUpdate;

    public VelopackAppUpdateService()
    {
        _updateManager = new UpdateManager(new GithubSource(RepositoryUrl, string.Empty, prerelease: false));
    }

    public async Task<AppUpdateCheckResult> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var currentVersion = GetCurrentVersion();

        if (!_updateManager.IsInstalled)
        {
            return new AppUpdateCheckResult(
                IsInstalled: false,
                IsUpdateAvailable: false,
                CurrentVersion: currentVersion,
                AvailableVersion: null,
                Message: "Atualizacoes ficam ativas depois de instalar pelo Setup.exe.");
        }

        var pendingRestart = _updateManager.UpdatePendingRestart;
        if (pendingRestart is not null)
        {
            return new AppUpdateCheckResult(
                IsInstalled: true,
                IsUpdateAvailable: true,
                CurrentVersion: currentVersion,
                AvailableVersion: pendingRestart.Version.ToString(),
                Message: $"Atualizacao {pendingRestart.Version} ja baixada. Reinicie para aplicar.");
        }

        _availableUpdate = await _updateManager.CheckForUpdatesAsync();
        cancellationToken.ThrowIfCancellationRequested();

        if (_availableUpdate is null)
        {
            return new AppUpdateCheckResult(
                IsInstalled: true,
                IsUpdateAvailable: false,
                CurrentVersion: currentVersion,
                AvailableVersion: null,
                Message: $"App atualizado na versao {currentVersion}.");
        }

        var targetVersion = _availableUpdate.TargetFullRelease.Version.ToString();
        return new AppUpdateCheckResult(
            IsInstalled: true,
            IsUpdateAvailable: true,
            CurrentVersion: currentVersion,
            AvailableVersion: targetVersion,
            Message: $"Atualizacao {targetVersion} disponivel.");
    }

    public async Task DownloadAndApplyUpdateAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var pendingRestart = _updateManager.UpdatePendingRestart;
        if (pendingRestart is not null)
        {
            _updateManager.ApplyUpdatesAndRestart(pendingRestart);
            return;
        }

        var update = _availableUpdate ?? await _updateManager.CheckForUpdatesAsync();
        cancellationToken.ThrowIfCancellationRequested();

        if (update is null)
        {
            return;
        }

        await _updateManager.DownloadUpdatesAsync(update, cancelToken: cancellationToken);
        _updateManager.ApplyUpdatesAndRestart(update.TargetFullRelease);
    }

    private string GetCurrentVersion()
    {
        return _updateManager.CurrentVersion?.ToString()
            ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)
            ?? "dev";
    }
}
