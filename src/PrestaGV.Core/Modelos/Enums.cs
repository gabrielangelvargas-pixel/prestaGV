namespace PrestaGV.Core.Modelos;

/// <summary>Cada cuánto se paga una cuota.</summary>
public enum FrecuenciaPago
{
    Semanal = 0,
    Quincenal = 1,
    Mensual = 2,
}

/// <summary>Cómo se reparte el interés entre las cuotas.</summary>
public enum TipoAmortizacion
{
    /// <summary>Cuota fija; el interés se calcula sobre el saldo pendiente (sistema francés).</summary>
    Frances = 0,

    /// <summary>Interés fijo sobre el monto original, repartido en partes iguales.</summary>
    InteresSimple = 1,
}

public enum EstadoPrestamo
{
    Activo = 0,
    Saldado = 1,
    Cancelado = 2,
}

public enum EstadoCuota
{
    Pendiente = 0,
    Parcial = 1,
    Pagada = 2,
    Vencida = 3,
}
