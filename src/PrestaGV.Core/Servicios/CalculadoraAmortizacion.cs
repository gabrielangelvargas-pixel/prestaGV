using PrestaGV.Core.Modelos;

namespace PrestaGV.Core.Servicios;

/// <summary>
/// Genera el calendario de cuotas de un préstamo. Todos los importes se redondean a 2 decimales
/// y la última cuota absorbe la diferencia de redondeo para que el capital cuadre exactamente.
/// </summary>
public static class CalculadoraAmortizacion
{
    public static int PeriodosPorAnio(FrecuenciaPago frecuencia) => frecuencia switch
    {
        FrecuenciaPago.Semanal => 52,
        FrecuenciaPago.Quincenal => 24,
        FrecuenciaPago.Mensual => 12,
        _ => throw new ArgumentOutOfRangeException(nameof(frecuencia)),
    };

    /// <summary>Convierte la tasa mensual (en %) a la tasa por período como fracción.</summary>
    public static decimal TasaPorPeriodo(decimal tasaMensualPorcentaje, FrecuenciaPago frecuencia) =>
        tasaMensualPorcentaje / 100m * 12m / PeriodosPorAnio(frecuencia);

    public static DateOnly FechaVencimiento(DateOnly inicio, FrecuenciaPago frecuencia, int numeroCuota) => frecuencia switch
    {
        FrecuenciaPago.Semanal => inicio.AddDays(7 * numeroCuota),
        FrecuenciaPago.Quincenal => inicio.AddDays(15 * numeroCuota),
        FrecuenciaPago.Mensual => inicio.AddMonths(numeroCuota),
        _ => throw new ArgumentOutOfRangeException(nameof(frecuencia)),
    };

    public static List<Cuota> GenerarCuotas(
        decimal monto,
        decimal tasaMensualPorcentaje,
        int numeroCuotas,
        FrecuenciaPago frecuencia,
        TipoAmortizacion tipo,
        DateOnly fechaInicio)
    {
        if (monto <= 0) throw new ArgumentOutOfRangeException(nameof(monto), "El monto debe ser mayor que cero.");
        if (tasaMensualPorcentaje < 0) throw new ArgumentOutOfRangeException(nameof(tasaMensualPorcentaje), "La tasa no puede ser negativa.");
        if (numeroCuotas <= 0) throw new ArgumentOutOfRangeException(nameof(numeroCuotas), "Debe haber al menos una cuota.");

        var tasa = TasaPorPeriodo(tasaMensualPorcentaje, frecuencia);
        var cuotas = tipo switch
        {
            TipoAmortizacion.Frances => Frances(monto, tasa, numeroCuotas),
            TipoAmortizacion.InteresSimple => InteresSimple(monto, tasa, numeroCuotas),
            _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
        };

        foreach (var cuota in cuotas)
            cuota.FechaVencimiento = FechaVencimiento(fechaInicio, frecuencia, cuota.Numero);

        return cuotas;
    }

    private static List<Cuota> Frances(decimal monto, decimal tasa, int n)
    {
        var cuotaFija = tasa == 0
            ? Redondear(monto / n)
            : Redondear(monto * tasa / (1 - 1 / Potencia(1 + tasa, n)));

        var cuotas = new List<Cuota>(n);
        var saldo = monto;
        for (var i = 1; i <= n; i++)
        {
            var interes = Redondear(saldo * tasa);
            var capital = i == n ? saldo : cuotaFija - interes;
            saldo -= capital;
            cuotas.Add(new Cuota { Numero = i, Capital = capital, Interes = interes, SaldoRestante = saldo });
        }

        return cuotas;
    }

    private static List<Cuota> InteresSimple(decimal monto, decimal tasa, int n)
    {
        var interesTotal = Redondear(monto * tasa * n);
        var capitalCuota = Redondear(monto / n);
        var interesCuota = Redondear(interesTotal / n);

        var cuotas = new List<Cuota>(n);
        var saldo = monto;
        for (var i = 1; i <= n; i++)
        {
            var ultima = i == n;
            var capital = ultima ? saldo : capitalCuota;
            var interes = ultima ? interesTotal - interesCuota * (n - 1) : interesCuota;
            saldo -= capital;
            cuotas.Add(new Cuota { Numero = i, Capital = capital, Interes = interes, SaldoRestante = saldo });
        }

        return cuotas;
    }

    private static decimal Potencia(decimal b, int exponente)
    {
        var resultado = 1m;
        for (var i = 0; i < exponente; i++) resultado *= b;
        return resultado;
    }

    private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
