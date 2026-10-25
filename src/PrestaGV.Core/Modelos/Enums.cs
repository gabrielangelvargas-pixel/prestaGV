namespace PrestaGV.Core.Modelos;

/// <summary>Etiqueta del plan. Los días reales entre cuotas se guardan en <see cref="Plan.DiasEntreCuotas"/>.</summary>
public enum FrecuenciaPago
{
    Semanal = 0,
    Quincenal = 1,
    Mensual = 2,
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
