using Microsoft.EntityFrameworkCore;
using PrestaGV.Core.Modelos;
using PrestaGV.Core.Servicios;
using PrestaGV.Data;

namespace PrestaGV.Core.Tests;

public sealed class BaseDeDatosTests : IDisposable
{
    private readonly string _ruta = Path.Combine(Path.GetTempPath(), $"prestagv-test-{Guid.NewGuid():N}.db");

    [Fact]
    public void Migraciones_PermitenGuardarYLeerUnPrestamoCompleto()
    {
        using (var db = new PrestaGVDbContext(BaseDeDatos.Opciones(_ruta)))
        {
            BaseDeDatos.Inicializar(db);
            var cliente = new Cliente { Nombres = "Luis", Apellidos = "Gómez", Documento = "402-0000000-1" };
            var prestamo = ServicioPrestamos.Crear(cliente, 5_000m, 5m, 6, FrecuenciaPago.Quincenal, TipoAmortizacion.Frances, new DateOnly(2026, 3, 1));
            ServicioPrestamos.RegistrarPago(prestamo, 1_000.55m, new DateOnly(2026, 3, 16));
            db.Prestamos.Add(prestamo);
            db.SaveChanges();
        }

        using (var db = new PrestaGVDbContext(BaseDeDatos.Opciones(_ruta)))
        {
            var prestamo = db.Prestamos.Include(p => p.Cliente).Include(p => p.Cuotas).Include(p => p.Pagos).Single();

            Assert.Equal("Luis Gómez", prestamo.Cliente!.NombreCompleto);
            Assert.Equal(FrecuenciaPago.Quincenal, prestamo.Frecuencia);
            Assert.Equal(6, prestamo.Cuotas.Count);
            Assert.Equal(5_000m, prestamo.Cuotas.Sum(c => c.Capital));
            Assert.Equal(1_000.55m, prestamo.TotalPagado);
        }
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Delete(_ruta);
    }
}
