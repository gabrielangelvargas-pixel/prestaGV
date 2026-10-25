using PrestaGV.Core.Modelos;
using PrestaGV.Core.Servicios;

namespace PrestaGV.Core.Tests;

public class CalculadoraCuotasTests
{
    private static readonly DateOnly Inicio = new(2026, 1, 15);

    [Fact]
    public void EjemploDelPlan_CincoCuotasRedondeadasYLaUltimaAjustada()
    {
        var cuotas = CalculadoraCuotas.Generar(10_000m, 60m, 6, 7, 10, Inicio);

        Assert.Equal([2670m, 2670m, 2670m, 2670m, 2670m, 2650m], cuotas.Select(c => c.Monto));
        Assert.Equal(16_000m, cuotas.Sum(c => c.Monto));
        Assert.Equal(10_000m, cuotas.Sum(c => c.Capital));
        Assert.Equal(6_000m, cuotas.Sum(c => c.Interes));
        Assert.Equal(0m, cuotas[^1].SaldoRestante);
    }

    [Fact]
    public void TotalADevolver_AplicaElInteresTotal()
    {
        Assert.Equal(16_000m, CalculadoraCuotas.TotalADevolver(10_000m, 60m));
        Assert.Equal(1_000m, CalculadoraCuotas.TotalADevolver(1_000m, 0m));
    }

    [Fact]
    public void Vencimientos_SeparadosPorLosDiasDelPlan()
    {
        var cuotas = CalculadoraCuotas.Generar(10_000m, 60m, 3, 15, 10, Inicio);

        Assert.Equal([new DateOnly(2026, 1, 30), new DateOnly(2026, 2, 14), new DateOnly(2026, 3, 1)],
            cuotas.Select(c => c.FechaVencimiento));
    }

    [Fact]
    public void SinRedondeo_RepartePartesIgualesConDosDecimales()
    {
        var cuotas = CalculadoraCuotas.Generar(1_000m, 0m, 3, 30, 0, Inicio);

        Assert.Equal([333.33m, 333.33m, 333.34m], cuotas.Select(c => c.Monto));
    }

    [Fact]
    public void CuotaExacta_NoNecesitaAjuste()
    {
        var cuotas = CalculadoraCuotas.Generar(5_000m, 20m, 4, 7, 10, Inicio);

        Assert.All(cuotas, c => Assert.Equal(1_500m, c.Monto));
    }

    [Fact]
    public void MontoPequeno_RedondeaHaciaAbajoParaQueLaUltimaNoQuedeEnCero()
    {
        // 100 / 6 = 16,67 → hacia arriba serían 5 x 20 = 100 y la última quedaría en 0.
        var cuotas = CalculadoraCuotas.Generar(100m, 0m, 6, 7, 10, Inicio);

        Assert.Equal([10m, 10m, 10m, 10m, 10m, 50m], cuotas.Select(c => c.Monto));
    }

    [Fact]
    public void UnaSolaCuota_EsElTotal()
    {
        var cuota = Assert.Single(CalculadoraCuotas.Generar(1_000m, 25m, 1, 30, 10, Inicio));

        Assert.Equal(1_250m, cuota.Monto);
        Assert.Equal(250m, cuota.Interes);
    }

    [Theory]
    [InlineData(0, 60, 6, 7, 10)]
    [InlineData(1000, -1, 6, 7, 10)]
    [InlineData(1000, 60, 0, 7, 10)]
    [InlineData(1000, 60, 6, 0, 10)]
    [InlineData(1000, 60, 6, 7, -10)]
    public void ParametrosInvalidos_LanzanExcepcion(decimal monto, decimal interes, int cuotas, int dias, int multiplo)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CalculadoraCuotas.Generar(monto, interes, cuotas, dias, multiplo, Inicio));
    }
}
