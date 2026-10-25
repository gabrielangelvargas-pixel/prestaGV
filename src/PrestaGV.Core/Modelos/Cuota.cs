namespace PrestaGV.Core.Modelos;

public class Cuota
{
    public int Id { get; set; }
    public int PrestamoId { get; set; }
    public Prestamo? Prestamo { get; set; }

    public int Numero { get; set; }
    public DateOnly FechaVencimiento { get; set; }
    public decimal Capital { get; set; }
    public decimal Interes { get; set; }
    public decimal MontoPagado { get; set; }

    /// <summary>Saldo de capital que queda después de pagar esta cuota.</summary>
    public decimal SaldoRestante { get; set; }

    public decimal Monto => Capital + Interes;
    public decimal Pendiente => Monto - MontoPagado;

    public EstadoCuota EstadoAl(DateOnly fecha) => this switch
    {
        { Pendiente: <= 0 } => EstadoCuota.Pagada,
        _ when FechaVencimiento < fecha => EstadoCuota.Vencida,
        { MontoPagado: > 0 } => EstadoCuota.Parcial,
        _ => EstadoCuota.Pendiente,
    };
}
