namespace PrestaGV.Core.Modelos;

public class Cliente
{
    public int Id { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>Cédula, DNI u otro documento de identidad. Único por cliente.</summary>
    public string Documento { get; set; } = string.Empty;

    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Notas { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public List<Prestamo> Prestamos { get; set; } = [];

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
}
