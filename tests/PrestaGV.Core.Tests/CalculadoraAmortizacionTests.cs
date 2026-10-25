using PrestaGV.Core.Modelos;
using PrestaGV.Core.Servicios;

namespace PrestaGV.Core.Tests;

public class CalculadoraAmortizacionTests
{
    private static readonly DateOnly Inicio = new(2026, 1, 15);

    [Fact]
    public void Frances_GeneraCuotaFijaYCuadraElCapital()
    {
        var cuotas = CalculadoraAmortizacion.GenerarCuotas(10_000m, 5m, 12, FrecuenciaPago.Mensual, TipoAmortizacion.Frances, Inicio);

        Assert.Equal(12, cuotas.Count);
        Assert.All(cuotas.Take(11), c => Assert.Equal(1128.25m, c.Monto));
        Assert.Equal(500m, cuotas[0].Interes);
        Assert.Equal(10_000m, cuotas.Sum(c => c.Capital));
        Assert.Equal(0m, cuotas[^1].SaldoRestante);
        // La última cuota absorbe los céntimos de redondeo acumulados.
        Assert.InRange(cuotas[^1].Monto, 1128.15m, 1128.35m);
    }

    [Fact]
    public void Frances_SinInteres_RepartePartesIguales()
    {
        var cuotas = CalculadoraAmortizacion.GenerarCuotas(1_000m, 0m, 3, FrecuenciaPago.Mensual, TipoAmortizacion.Frances, Inicio);

        Assert.Equal([333.33m, 333.33m, 333.34m], cuotas.Select(c => c.Monto));
        Assert.All(cuotas, c => Assert.Equal(0m, c.Interes));
    }

    [Fact]
    public void InteresSimple_CalculaInteresSobreElMontoOriginal()
    {
        var cuotas = CalculadoraAmortizacion.GenerarCuotas(10_000m, 10m, 4, FrecuenciaPago.Mensual, TipoAmortizacion.InteresSimple, Inicio);

        Assert.All(cuotas, c =>
        {
            Assert.Equal(2_500m, c.Capital);
            Assert.Equal(1_000m, c.Interes);
        });
        Assert.Equal(14_000m, cuotas.Sum(c => c.Monto));
    }

    [Fact]
    public void InteresSimple_LaUltimaCuotaAbsorbeElRedondeo()
    {
        var cuotas = CalculadoraAmortizacion.GenerarCuotas(1_000m, 10m, 3, FrecuenciaPago.Mensual, TipoAmortizacion.InteresSimple, Inicio);

        Assert.Equal(1_000m, cuotas.Sum(c => c.Capital));
        Assert.Equal(300m, cuotas.Sum(c => c.Interes));
    }

    [Theory]
    [InlineData(FrecuenciaPago.Semanal, 2026, 1, 22)]
    [InlineData(FrecuenciaPago.Quincenal, 2026, 1, 30)]
    [InlineData(FrecuenciaPago.Mensual, 2026, 2, 15)]
    public void PrimeraCuota_VenceUnPeriodoDespuesDelInicio(FrecuenciaPago frecuencia, int anio, int mes, int dia)
    {
        var cuotas = CalculadoraAmortizacion.GenerarCuotas(1_000m, 5m, 2, frecuencia, TipoAmortizacion.Frances, Inicio);

        Assert.Equal(new DateOnly(anio, mes, dia), cuotas[0].FechaVencimiento);
    }

    [Fact]
    public void TasaPorPeriodo_ConvierteLaTasaMensual()
    {
        Assert.Equal(0.05m, CalculadoraAmortizacion.TasaPorPeriodo(5m, FrecuenciaPago.Mensual));
        Assert.Equal(0.025m, CalculadoraAmortizacion.TasaPorPeriodo(5m, FrecuenciaPago.Quincenal));
    }

    [Theory]
    [InlineData(0, 5, 12)]
    [InlineData(1000, -1, 12)]
    [InlineData(1000, 5, 0)]
    public void ParametrosInvalidos_LanzanExcepcion(decimal monto, decimal tasa, int cuotas)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CalculadoraAmortizacion.GenerarCuotas(monto, tasa, cuotas, FrecuenciaPago.Mensual, TipoAmortizacion.Frances, Inicio));
    }
}
