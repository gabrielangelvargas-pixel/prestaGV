namespace PrestaGV.Core.Modelos;

public class Pago
{
    public int Id { get; set; }
    public int PrestamoId { get; set; }
    public Prestamo? Prestamo { get; set; }

    public DateOnly Fecha { get; set; }
    public decimal Monto { get; set; }
    public string? Nota { get; set; }
}
