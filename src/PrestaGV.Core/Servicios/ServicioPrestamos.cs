using PrestaGV.Core.Modelos;

namespace PrestaGV.Core.Servicios;

public static class ServicioPrestamos
{
    /// <summary>Crea un préstamo con las condiciones del plan y su calendario de cuotas ya calculado.</summary>
    public static Prestamo Crear(Cliente cliente, Plan plan, decimal monto, DateOnly fechaInicio)
    {
        return new Prestamo
        {
            Cliente = cliente,
            ClienteId = cliente.Id,
            Plan = plan,
            PlanId = plan.Id,
            Monto = monto,
            InteresTotalPorcentaje = plan.InteresTotalPorcentaje,
            NumeroCuotas = plan.NumeroCuotas,
            Frecuencia = plan.Frecuencia,
            DiasEntreCuotas = plan.DiasEntreCuotas,
            MultiploRedondeo = plan.MultiploRedondeo,
            FechaInicio = fechaInicio,
            Cuotas = CalculadoraCuotas.Generar(monto, plan, fechaInicio),
        };
    }

    /// <summary>
    /// Registra un pago y lo aplica a las cuotas pendientes, empezando por la más antigua.
    /// Marca el préstamo como saldado cuando ya no queda nada pendiente.
    /// </summary>
    public static Pago RegistrarPago(Prestamo prestamo, decimal monto, DateOnly fecha, string? nota = null)
    {
        if (prestamo.Estado != EstadoPrestamo.Activo)
            throw new InvalidOperationException("Solo se pueden registrar pagos en préstamos activos.");
        if (monto <= 0)
            throw new ArgumentOutOfRangeException(nameof(monto), "El pago debe ser mayor que cero.");
        if (monto > prestamo.SaldoPendiente)
            throw new ArgumentOutOfRangeException(nameof(monto), $"El pago supera el saldo pendiente ({prestamo.SaldoPendiente:N2}).");

        var restante = monto;
        foreach (var cuota in prestamo.Cuotas.Where(c => c.Pendiente > 0).OrderBy(c => c.Numero))
        {
            var aplicado = Math.Min(restante, cuota.Pendiente);
            cuota.MontoPagado += aplicado;
            restante -= aplicado;
            if (restante == 0) break;
        }

        var pago = new Pago { Prestamo = prestamo, PrestamoId = prestamo.Id, Fecha = fecha, Monto = monto, Nota = nota };
        prestamo.Pagos.Add(pago);

        if (prestamo.SaldoPendiente == 0)
            prestamo.Estado = EstadoPrestamo.Saldado;

        return pago;
    }
}
