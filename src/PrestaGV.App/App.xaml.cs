using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using PrestaGV.App.Servicios;
using PrestaGV.App.ViewModels;
using PrestaGV.Data;

namespace PrestaGV.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += AlFallar;

        // Fechas siempre como dd/MM/yyyy (por ejemplo 05/10/2026), sea cual sea la configuración de Windows.
        // Los selectores de fecha usan la cultura del hilo; las tablas lo indican en su StringFormat.
        var cultura = (CultureInfo)CultureInfo.CurrentCulture.Clone();
        cultura.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
        CultureInfo.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture = cultura;

        // WPF formatea con en-US por defecto; usamos la configuración regional de Windows.
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));

        var opciones = BaseDeDatos.Opciones();
        using (var db = new PrestaGVDbContext(opciones))
            BaseDeDatos.Inicializar(db);

        var ventana = new MainWindow
        {
            DataContext = new MainViewModel(() => new PrestaGVDbContext(opciones), new ServicioActualizaciones()),
        };
        ventana.Show();
    }

    private static void AlFallar(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "PrestaGV", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
