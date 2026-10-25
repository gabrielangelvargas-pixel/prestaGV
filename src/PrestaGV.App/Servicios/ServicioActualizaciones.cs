using Velopack;
using Velopack.Sources;

namespace PrestaGV.App.Servicios;

/// <summary>Busca, descarga y aplica actualizaciones publicadas en GitHub Releases.</summary>
public sealed class ServicioActualizaciones
{
    private const string UrlRepositorio = "https://github.com/gabrielangelvargas-pixel/prestaGV";

    private readonly UpdateManager _manager = new(new GithubSource(UrlRepositorio, accessToken: null, prerelease: false));
    private UpdateInfo? _pendiente;

    /// <summary>False cuando se ejecuta desde Visual Studio en vez de desde el instalador.</summary>
    public bool EstaInstalada => _manager.IsInstalled;

    public string VersionActual => _manager.CurrentVersion?.ToString() ?? "desarrollo";

    /// <summary>Devuelve la versión nueva ya descargada, o null si no hay ninguna.</summary>
    public async Task<string?> BuscarYDescargarAsync()
    {
        if (!EstaInstalada) return null;

        _pendiente = await _manager.CheckForUpdatesAsync();
        if (_pendiente is null) return null;

        await _manager.DownloadUpdatesAsync(_pendiente);
        return _pendiente.TargetFullRelease.Version.ToString();
    }

    public void AplicarYReiniciar()
    {
        if (_pendiente is not null)
            _manager.ApplyUpdatesAndRestart(_pendiente.TargetFullRelease);
    }
}
