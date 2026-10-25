using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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
            var plan = new Plan { Nombre = "Quincenal 6", Frecuencia = FrecuenciaPago.Quincenal, NumeroCuotas = 6, DiasEntreCuotas = 15, InteresTotalPorcentaje = 30m };
            var prestamo = ServicioPrestamos.Crear(cliente, plan, 5_000m, new DateOnly(2026, 3, 1));
            ServicioPrestamos.RegistrarPago(prestamo, 1_000.55m, new DateOnly(2026, 3, 16));
            db.Prestamos.Add(prestamo);
            db.SaveChanges();
        }

        using (var db = new PrestaGVDbContext(BaseDeDatos.Opciones(_ruta)))
        {
            var prestamo = db.Prestamos.Include(p => p.Cliente).Include(p => p.Plan).Include(p => p.Cuotas).Include(p => p.Pagos).Single();

            Assert.Equal("Luis Gómez", prestamo.Cliente!.NombreCompleto);
            Assert.Equal("Quincenal 6", prestamo.Plan!.Nombre);
            Assert.Equal(FrecuenciaPago.Quincenal, prestamo.Frecuencia);
            Assert.Equal(6, prestamo.Cuotas.Count);
            Assert.Equal(5_000m, prestamo.Cuotas.Sum(c => c.Capital));
            Assert.Equal(6_500m, prestamo.TotalAPagar);
            Assert.Equal(1_000.55m, prestamo.TotalPagado);
        }
    }

    [Fact]
    public void BaseNueva_IncluyeElPlanDeEjemplo()
    {
        using var db = new PrestaGVDbContext(BaseDeDatos.Opciones(_ruta));
        BaseDeDatos.Inicializar(db);

        var plan = db.Planes.Single();
        Assert.Equal(60m, plan.InteresTotalPorcentaje);
        Assert.Equal(6, plan.NumeroCuotas);
        Assert.Equal(7, plan.DiasEntreCuotas);
    }

    [Fact]
    public void ActualizarDesdeLaVersion010_ConservaLosPrestamosExistentes()
    {
        using (var db = new PrestaGVDbContext(BaseDeDatos.Opciones(_ruta)))
        {
            db.GetService<IMigrator>().Migrate("20261025183928_Inicial");
            db.Database.ExecuteSqlRaw("""
                INSERT INTO Clientes (Id, Nombres, Apellidos, Documento, FechaRegistro) VALUES (1, 'Ana', 'Pérez', '001', '2026-10-25');
                INSERT INTO Prestamos (Id, ClienteId, Monto, TasaInteresMensual, NumeroCuotas, Frecuencia, TipoAmortizacion, FechaInicio, Estado)
                    VALUES (1, 1, '1000.0', '5.0', 2, 'Quincenal', 'Frances', '2026-10-25', 'Activo');
                INSERT INTO Cuotas (PrestamoId, Numero, FechaVencimiento, Capital, Interes, MontoPagado, SaldoRestante) VALUES
                    (1, 1, '2026-11-09', '480.0', '25.0', '0.0', '520.0'),
                    (1, 2, '2026-11-24', '520.0', '13.0', '0.0', '0.0');
                """);
            BaseDeDatos.Inicializar(db);
        }

        using (var db = new PrestaGVDbContext(BaseDeDatos.Opciones(_ruta)))
        {
            var prestamo = db.Prestamos.Include(p => p.Cuotas).Single();
            Assert.Null(prestamo.PlanId);
            Assert.Equal(3.8m, prestamo.InteresTotalPorcentaje);
            Assert.Equal(15, prestamo.DiasEntreCuotas);
            Assert.Equal(1_038m, prestamo.TotalAPagar);
        }
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        File.Delete(_ruta);
    }
}
