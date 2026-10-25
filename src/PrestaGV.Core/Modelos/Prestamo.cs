namespace PrestaGV.Core.Modelos;

public class Prestamo
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public decimal Monto { get; set; }

    /// <summary>Tasa de interés mensual en porcentaje (por ejemplo 5 = 5 % mensual).</summary>
    public decimal TasaInteresMensual { get; set; }

    public int NumeroCuotas { get; set; }
    public FrecuenciaPago Frecuencia { get; set; }
    public TipoAmortizacion TipoAmortizacion { get; set; }
    public DateOnly FechaInicio { get; set; }
    public EstadoPrestamo Estado { get; set; } = EstadoPrestamo.Activo;
    public string? Notas { get; set; }

    public List<Cuota> Cuotas { get; set; } = [];
    public List<Pago> Pagos { get; set; } = [];

    public decimal TotalAPagar => Cuotas.Sum(c => c.Monto);
    public decimal TotalPagado => Cuotas.Sum(c => c.MontoPagado);
    public decimal SaldoPendiente => TotalAPagar - TotalPagado;
}
