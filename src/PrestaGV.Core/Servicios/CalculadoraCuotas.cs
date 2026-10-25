using PrestaGV.Core.Modelos;

namespace PrestaGV.Core.Servicios;

/// <summary>
/// Genera el calendario de cuotas de un plan con interés total fijo.
/// Todas las cuotas menos la última se redondean hacia arriba al múltiplo del plan
/// y la última se ajusta a lo que falta: 10.000 al 60 % en 6 cuotas → 5 x 2.670 y 1 x 2.650.
/// </summary>
public static class CalculadoraCuotas
{
    public static decimal TotalADevolver(decimal monto, decimal interesTotalPorcentaje) =>
        Redondear(monto * (1 + interesTotalPorcentaje / 100m));

    public static List<Cuota> Generar(decimal monto, Plan plan, DateOnly fechaInicio) =>
        Generar(monto, plan.InteresTotalPorcentaje, plan.NumeroCuotas, plan.DiasEntreCuotas, plan.MultiploRedondeo, fechaInicio);

    public static List<Cuota> Generar(
        decimal monto,
        decimal interesTotalPorcentaje,
        int numeroCuotas,
        int diasEntreCuotas,
        int multiploRedondeo,
        DateOnly fechaInicio)
    {
        if (monto <= 0) throw new ArgumentOutOfRangeException(nameof(monto), "El monto debe ser mayor que cero.");
        if (interesTotalPorcentaje < 0) throw new ArgumentOutOfRangeException(nameof(interesTotalPorcentaje), "El interés no puede ser negativo.");
        if (numeroCuotas <= 0) throw new ArgumentOutOfRangeException(nameof(numeroCuotas), "Debe haber al menos una cuota.");
        if (diasEntreCuotas <= 0) throw new ArgumentOutOfRangeException(nameof(diasEntreCuotas), "Los días entre cuotas deben ser al menos 1.");
        if (multiploRedondeo < 0) throw new ArgumentOutOfRangeException(nameof(multiploRedondeo), "El múltiplo de redondeo no puede ser negativo.");

        var total = TotalADevolver(monto, interesTotalPorcentaje);
        var interesTotal = total - monto;
        var importes = Importes(total, numeroCuotas, multiploRedondeo);

        var cuotas = new List<Cuota>(numeroCuotas);
        var capitalRestante = monto;
        var interesRestante = interesTotal;
        for (var i = 0; i < numeroCuotas; i++)
        {
            var ultima = i == numeroCuotas - 1;
            // El interés de cada cuota es proporcional a su importe; la última se queda con el resto.
            var interes = ultima ? interesRestante : Redondear(importes[i] * interesTotal / total);
            var capital = ultima ? capitalRestante : importes[i] - interes;
            capitalRestante -= capital;
            interesRestante -= interes;
            cuotas.Add(new Cuota
            {
                Numero = i + 1,
                FechaVencimiento = fechaInicio.AddDays(diasEntreCuotas * (i + 1)),
                Capital = capital,
                Interes = interes,
                SaldoRestante = capitalRestante,
            });
        }

        return cuotas;
    }

    /// <summary>Importe de cada cuota: las primeras redondeadas hacia arriba, la última ajustada.</summary>
    private static decimal[] Importes(decimal total, int n, int multiplo)
    {
        var importes = new decimal[n];
        if (n == 1)
        {
            importes[0] = total;
            return importes;
        }

        var cuota = RedondearCuota(total / n, multiplo, Math.Ceiling);
        // Con montos pequeños, redondear hacia arriba puede dejar la última cuota en cero o negativa.
        if (cuota * (n - 1) >= total)
            cuota = RedondearCuota(total / n, multiplo, Math.Floor);

        for (var i = 0; i < n - 1; i++) importes[i] = cuota;
        importes[^1] = total - cuota * (n - 1);
        return importes;
    }

    private static decimal RedondearCuota(decimal valor, int multiplo, Func<decimal, decimal> redondeo) =>
        multiplo > 0 ? redondeo(valor / multiplo) * multiplo : Redondear(valor);

    private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);
}
