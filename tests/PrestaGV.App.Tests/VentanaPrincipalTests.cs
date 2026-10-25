using System.IO;
using System.Runtime.ExceptionServices;
using PrestaGV.App.Servicios;
using PrestaGV.App.ViewModels;
using PrestaGV.Data;
using Velopack;

namespace PrestaGV.App.Tests;

/// <summary>
/// Carga los recursos de la app y construye la ventana principal sin mostrarla.
/// Detecta en la CI (Windows) estilos o recursos que no existen, que de otro modo
/// solo fallarían al abrir la aplicación.
/// </summary>
public sealed class VentanaPrincipalTests : IDisposable
{
    private readonly string _ruta = Path.Combine(Path.GetTempPath(), $"prestagv-ui-{Guid.NewGuid():N}.db");

    [Fact]
    public void LaVentanaPrincipalSeConstruyeConLosEstilos()
    {
        // Igual que Program.Main: Velopack debe inicializarse antes de crear el UpdateManager.
        VelopackApp.Build().Run();

        EnHiloSta(() =>
        {
            var app = new App();
            app.InitializeComponent();

            var opciones = BaseDeDatos.Opciones(_ruta);
            using (var db = new PrestaGVDbContext(opciones))
                BaseDeDatos.Inicializar(db);

            var ventana = new MainWindow
            {
                DataContext = new MainViewModel(() => new PrestaGVDbContext(opciones), new ServicioActualizaciones()),
            };
            ventana.Measure(new System.Windows.Size(1280, 760));
            ventana.Arrange(new System.Windows.Rect(0, 0, 1280, 760));
            ventana.UpdateLayout();
            ventana.Close();
            app.Shutdown();
        });
    }

    private static void EnHiloSta(Action accion)
    {
        ExceptionDispatchInfo? error = null;
        var hilo = new Thread(() =>
        {
            try { accion(); }
            catch (Exception ex) { error = ExceptionDispatchInfo.Capture(ex); }
        });
        hilo.SetApartmentState(ApartmentState.STA);
        hilo.Start();
        hilo.Join();
        error?.Throw();
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Delete(_ruta);
    }
}
