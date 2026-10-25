using Microsoft.EntityFrameworkCore;

namespace PrestaGV.Data;

public static class BaseDeDatos
{
    /// <summary>
    /// Ruta del archivo SQLite: %LocalAppData%\PrestaGV\prestagv.db.
    /// Vive fuera de la carpeta de instalación para que las actualizaciones no la borren.
    /// </summary>
    public static string RutaPorDefecto
    {
        get
        {
            var carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PrestaGV");
            Directory.CreateDirectory(carpeta);
            return Path.Combine(carpeta, "prestagv.db");
        }
    }

    public static DbContextOptions<PrestaGVDbContext> Opciones(string? ruta = null) =>
        new DbContextOptionsBuilder<PrestaGVDbContext>()
            .UseSqlite($"Data Source={ruta ?? RutaPorDefecto}")
            .Options;

    /// <summary>Crea la base de datos si no existe y aplica las migraciones pendientes.</summary>
    public static void Inicializar(PrestaGVDbContext db) => db.Database.Migrate();
}
