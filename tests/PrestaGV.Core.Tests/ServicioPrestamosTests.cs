using PrestaGV.Core.Modelos;
using PrestaGV.Core.Servicios;

namespace PrestaGV.Core.Tests;

public class ServicioPrestamosTests
{
    // 10.000 al 40 % en 4 cuotas mensuales → 4 x 3.500.
    private static readonly Plan Plan = new()
    {
        Id = 7, Nombre = "Mensual 4", Frecuencia = FrecuenciaPago.Mensual, NumeroCuotas = 4,
        DiasEntreCuotas = 31, InteresTotalPorcentaje = 40m, MultiploRedondeo = 10,
    };

    private static Prestamo NuevoPrestamo() => ServicioPrestamos.Crear(
        new Cliente { Nombres = "Ana", Apellidos = "Pérez", Documento = "001" }, Plan, 10_000m, new DateOnly(2026, 1, 1));

    [Fact]
    public void Crear_CopiaLasCondicionesDelPlan()
    {
        var prestamo = NuevoPrestamo();

        Assert.Equal(7, prestamo.PlanId);
        Assert.Equal(40m, prestamo.InteresTotalPorcentaje);
        Assert.Equal(4, prestamo.NumeroCuotas);
        Assert.Equal(31, prestamo.DiasEntreCuotas);
        Assert.Equal(14_000m, prestamo.TotalAPagar);
    }

    [Fact]
    public void Pago_SeAplicaALaCuotaMasAntigua()
    {
        var prestamo = NuevoPrestamo();

        ServicioPrestamos.RegistrarPago(prestamo, 5_000m, new DateOnly(2026, 2, 1));

        Assert.Equal(3_500m, prestamo.Cuotas[0].MontoPagado);
        Assert.Equal(1_500m, prestamo.Cuotas[1].MontoPagado);
        Assert.Equal(0m, prestamo.Cuotas[2].MontoPagado);
        Assert.Equal(9_000m, prestamo.SaldoPendiente);
        Assert.Single(prestamo.Pagos);
    }

    [Fact]
    public void PagoTotal_SaldaElPrestamo()
    {
        var prestamo = NuevoPrestamo();

        ServicioPrestamos.RegistrarPago(prestamo, 14_000m, new DateOnly(2026, 2, 1));

        Assert.Equal(EstadoPrestamo.Saldado, prestamo.Estado);
        Assert.Throws<InvalidOperationException>(() => ServicioPrestamos.RegistrarPago(prestamo, 1m, new DateOnly(2026, 2, 2)));
    }

    [Fact]
    public void PagoMayorAlSaldo_SeRechaza()
    {
        var prestamo = NuevoPrestamo();

        Assert.Throws<ArgumentOutOfRangeException>(() => ServicioPrestamos.RegistrarPago(prestamo, 14_000.01m, new DateOnly(2026, 2, 1)));
        Assert.Empty(prestamo.Pagos);
    }

    [Fact]
    public void EstadoCuota_DependeDeLaFechaYLoPagado()
    {
        var prestamo = NuevoPrestamo();
        var cuota = prestamo.Cuotas[0]; // vence el 2026-02-01

        Assert.Equal(EstadoCuota.Pendiente, cuota.EstadoAl(new DateOnly(2026, 1, 20)));
        Assert.Equal(EstadoCuota.Vencida, cuota.EstadoAl(new DateOnly(2026, 2, 2)));

        ServicioPrestamos.RegistrarPago(prestamo, 1_000m, new DateOnly(2026, 1, 20));
        Assert.Equal(EstadoCuota.Parcial, cuota.EstadoAl(new DateOnly(2026, 1, 20)));

        ServicioPrestamos.RegistrarPago(prestamo, 2_500m, new DateOnly(2026, 1, 25));
        Assert.Equal(EstadoCuota.Pagada, cuota.EstadoAl(new DateOnly(2026, 3, 1)));
    }
}
