using Velopack;

namespace PrestaGV.App;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        // Debe ser lo primero: Velopack gestiona aquí los eventos de instalación,
        // actualización y desinstalación, y puede terminar el proceso.
        VelopackApp.Build().Run();

        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}
