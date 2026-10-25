using PrestaGV.Core.Modelos;

namespace PrestaGV.App.ViewModels;

/// <summary>Una cuota tal como se muestra en las tablas, con su estado calculado a hoy.</summary>
public record CuotaFila(int Numero, DateOnly Vence, decimal Capital, decimal Interes, decimal Monto, decimal Pagado, decimal Pendiente, EstadoCuota Estado)
{
    public static CuotaFila Desde(Cuota c) =>
        new(c.Numero, c.FechaVencimiento, c.Capital, c.Interes, c.Monto, c.MontoPagado, c.Pendiente, c.EstadoAl(DateOnly.FromDateTime(DateTime.Today)));
}
