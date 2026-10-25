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
        CargarPlanes();
        CargarPrestamos();
    }

    public ObservableCollection<Cliente> Clientes { get; } = [];
    public ObservableCollection<Plan> Planes { get; } = [];
    public ObservableCollection<Prestamo> Lista { get; } = [];
    public ObservableCollection<CuotaFila> CuotasSeleccionado { get; } = [];
    public ObservableCollection<CuotaFila> VistaPrevia { get; } = [];

    // Formulario de nuevo préstamo
    [ObservableProperty] private Cliente? _cliente;
    [ObservableProperty] private Plan? _plan;
    [ObservableProperty] private decimal _monto = 10_000m;
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

    public void CargarPlanes()
    {
        var seleccionadoId = Plan?.Id;
        using var db = _crearDb();
        Planes.Clear();
        foreach (var p in db.Planes.AsNoTracking().Where(p => p.Activo).OrderBy(p => p.Nombre))
            Planes.Add(p);
        Plan = Planes.FirstOrDefault(p => p.Id == seleccionadoId) ?? Planes.FirstOrDefault();
    }

    private void CargarPrestamos(int? seleccionarId = null)
    {
        using var db = _crearDb();
        var prestamos = db.Prestamos.AsNoTracking()
            .Include(p => p.Cliente)
            .Include(p => p.Plan)
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
        if (Plan is null)
        {
            ResumenVistaPrevia = "Selecciona un plan.";
            return;
        }

        try
        {
            var cuotas = CalculadoraCuotas.Generar(Monto, Plan, DateOnly.FromDateTime(FechaInicio));
            foreach (var c in cuotas) VistaPrevia.Add(CuotaFila.Desde(c));
            ResumenVistaPrevia = $"Total a devolver: {cuotas.Sum(c => c.Monto):N2}\n{Formato.Cuotas(cuotas)}";
        }
        catch (ArgumentException ex)
        {
            ResumenVistaPrevia = ex.Message;
        }
    }

    [RelayCommand]
    private void Crear()
    {
        if (Cliente is null || Plan is null)
        {
            MessageBox.Show("Selecciona un cliente y un plan.", "Préstamos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = _crearDb();
        var cliente = db.Clientes.Find(Cliente.Id)!;
        var plan = db.Planes.Find(Plan.Id)!;
        Prestamo prestamo;
        try
        {
            prestamo = ServicioPrestamos.Crear(cliente, plan, Monto, DateOnly.FromDateTime(FechaInicio));
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "Préstamos", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
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
