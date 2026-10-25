using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PrestaGV.Core.Modelos;
using PrestaGV.Core.Servicios;
using PrestaGV.Data;

namespace PrestaGV.App.ViewModels;

public partial class PrestamosViewModel : ObservableObject
{
    private readonly Func<PrestaGVDbContext> _crearDb;

    public PrestamosViewModel(Func<PrestaGVDbContext> crearDb)
    {
        _crearDb = crearDb;
        CargarClientes();
        CargarPrestamos();
    }

    public ObservableCollection<Cliente> Clientes { get; } = [];
    public ObservableCollection<Prestamo> Lista { get; } = [];
    public ObservableCollection<CuotaFila> CuotasSeleccionado { get; } = [];
    public ObservableCollection<CuotaFila> VistaPrevia { get; } = [];

    public FrecuenciaPago[] Frecuencias { get; } = Enum.GetValues<FrecuenciaPago>();
    public TipoAmortizacion[] Tipos { get; } = Enum.GetValues<TipoAmortizacion>();

    // Formulario de nuevo préstamo
    [ObservableProperty] private Cliente? _cliente;
    [ObservableProperty] private decimal _monto = 10_000m;
    [ObservableProperty] private decimal _tasaMensual = 5m;
    [ObservableProperty] private int _numeroCuotas = 12;
    [ObservableProperty] private FrecuenciaPago _frecuencia = FrecuenciaPago.Mensual;
    [ObservableProperty] private TipoAmortizacion _tipo = TipoAmortizacion.Frances;
    [ObservableProperty] private DateTime _fechaInicio = DateTime.Today;
    [ObservableProperty] private string _resumenVistaPrevia = string.Empty;

    // Préstamo seleccionado y registro de pagos
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegistrarPagoCommand))]
    private Prestamo? _seleccionado;

    [ObservableProperty] private decimal _montoPago;
    [ObservableProperty] private DateTime _fechaPago = DateTime.Today;

    partial void OnSeleccionadoChanged(Prestamo? value)
    {
        CuotasSeleccionado.Clear();
        if (value is null) return;
        foreach (var cuota in value.Cuotas.OrderBy(c => c.Numero))
            CuotasSeleccionado.Add(CuotaFila.Desde(cuota));
        MontoPago = value.Cuotas.OrderBy(c => c.Numero).FirstOrDefault(c => c.Pendiente > 0)?.Pendiente ?? 0m;
    }

    public void CargarClientes()
    {
        using var db = _crearDb();
        Clientes.Clear();
        foreach (var c in db.Clientes.AsNoTracking().OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres))
            Clientes.Add(c);
    }

    private void CargarPrestamos(int? seleccionarId = null)
    {
        using var db = _crearDb();
        var prestamos = db.Prestamos.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Cuotas)
            .OrderByDescending(p => p.Id)
            .ToList();

        Lista.Clear();
        foreach (var p in prestamos) Lista.Add(p);
        Seleccionado = Lista.FirstOrDefault(p => p.Id == seleccionarId);
    }

    [RelayCommand]
    private void Calcular()
    {
        VistaPrevia.Clear();
        try
        {
            var cuotas = CalculadoraAmortizacion.GenerarCuotas(Monto, TasaMensual, NumeroCuotas, Frecuencia, Tipo, DateOnly.FromDateTime(FechaInicio));
            foreach (var c in cuotas) VistaPrevia.Add(CuotaFila.Desde(c));
            ResumenVistaPrevia = $"Total a pagar: {cuotas.Sum(c => c.Monto):N2}   Intereses: {cuotas.Sum(c => c.Interes):N2}";
        }
        catch (ArgumentException ex)
        {
            ResumenVistaPrevia = ex.Message;
        }
    }

    [RelayCommand]
    private void Crear()
    {
        if (Cliente is null)
        {
            MessageBox.Show("Selecciona un cliente.", "Préstamos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = _crearDb();
        var cliente = db.Clientes.Find(Cliente.Id)!;
        var prestamo = ServicioPrestamos.Crear(cliente, Monto, TasaMensual, NumeroCuotas, Frecuencia, Tipo, DateOnly.FromDateTime(FechaInicio));
        db.Prestamos.Add(prestamo);
        db.SaveChanges();

        VistaPrevia.Clear();
        ResumenVistaPrevia = string.Empty;
        CargarPrestamos(prestamo.Id);
    }

    private bool PuedeRegistrarPago() => Seleccionado is { Estado: EstadoPrestamo.Activo };

    [RelayCommand(CanExecute = nameof(PuedeRegistrarPago))]
    private void RegistrarPago()
    {
        using var db = _crearDb();
        var prestamo = db.Prestamos.Include(p => p.Cuotas).Single(p => p.Id == Seleccionado!.Id);
        try
        {
            ServicioPrestamos.RegistrarPago(prestamo, MontoPago, DateOnly.FromDateTime(FechaPago));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            MessageBox.Show(ex.Message, "Pagos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        db.SaveChanges();
        CargarPrestamos(prestamo.Id);
    }
}
