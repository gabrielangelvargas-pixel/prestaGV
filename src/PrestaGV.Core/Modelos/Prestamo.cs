namespace PrestaGV.Core.Modelos;

public class Prestamo
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    /// <summary>Plan con el que se creó. Sus condiciones se copian abajo para que editar el plan no altere préstamos existentes.</summary>
    public int? PlanId { get; set; }
    public Plan? Plan { get; set; }

    public decimal Monto { get; set; }
    public decimal InteresTotalPorcentaje { get; set; }
    public int NumeroCuotas { get; set; }
    public FrecuenciaPago Frecuencia { get; set; }
    public int DiasEntreCuotas { get; set; }
    public int MultiploRedondeo { get; set; }
    public DateOnly FechaInicio { get; set; }
    public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.Activo;
    public string? Notas { get; set; }

    public List<Cuota> Cuotas { get; set; } = [];
    public List<Pago> Pagos { get; set; } = [];

    public decimal TotalAPagar => Cuotas.Sum(c => c.Monto);
    public decimal TotalPagado => Cuotas.Sum(c => c.MontoPagado);
    public decimal SaldoPendiente => TotalAPagar - TotalPagado;
}
