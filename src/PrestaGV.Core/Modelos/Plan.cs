namespace PrestaGV.Core.Modelos;

/// <summary>
/// Plantilla de préstamo: cuántas cuotas, cada cuántos días y qué interés total se cobra.
/// Ejemplo: 6 cuotas semanales con 60 % de interés → 10.000 se devuelven como 16.000.
/// </summary>
public class Plan
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public FrecuenciaPago Frecuencia { get; set; }
    public int NumeroCuotas { get; set; }
    public int DiasEntreCuotas { get; set; }

    /// <summary>Interés total del plan en porcentaje sobre el monto prestado (60 = se devuelve el 160 %).</summary>
    public decimal InteresTotalPorcentaje { get; set; }

    /// <summary>Las cuotas se redondean hacia arriba a este múltiplo (10 → 2.670). 0 = sin redondeo.</summary>
    public int MultiploRedondeo { get; set; } = 10;

    /// <summary>Los planes inactivos no se ofrecen para préstamos nuevos.</summary>
    public bool Activo { get; set; } = true;

    public static int DiasPorDefecto(FrecuenciaPago frecuencia) => frecuencia switch
    {
        FrecuenciaPago.Semanal => 7,
        FrecuenciaPago.Quincenal => 15,
        FrecuenciaPago.Mensual => 30,
        _ => throw new ArgumentOutOfRangeException(nameof(frecuencia)),
    };
}
