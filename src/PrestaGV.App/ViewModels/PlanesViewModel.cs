using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PrestaGV.Core.Modelos;
using PrestaGV.Core.Servicios;
using PrestaGV.Data;

namespace PrestaGV.App.ViewModels;

public partial class PlanesViewModel : ObservableObject
{
    private readonly Func<PrestaGVDbContext> _crearDb;

    public PlanesViewModel(Func<PrestaGVDbContext> crearDb)
    {
        _crearDb = crearDb;
        Cargar();
        Nuevo();
    }

    public event EventHandler? PlanGuardado;

    public ObservableCollection<Plan> Lista { get; } = [];
    public ObservableCollection<CuotaFila> Ejemplo { get; } = [];
    public FrecuenciaPago[] Frecuencias { get; } = Enum.GetValues<FrecuenciaPago>();

    [ObservableProperty] private Plan? _seleccionado;
    [ObservableProperty] private string _titulo = string.Empty;
    [ObservableProperty] private string _nombre = string.Empty;
    [ObservableProperty] private FrecuenciaPago _frecuencia;
    [ObservableProperty] private int _numeroCuotas;
    [ObservableProperty] private int _diasEntreCuotas;
    [ObservableProperty] private decimal _interesTotal;
    [ObservableProperty] private int _multiploRedondeo;
    [ObservableProperty] private bool _activo;
    [ObservableProperty] private decimal _montoEjemplo = 10_000m;
    [ObservableProperty] private string _resumenEjemplo = string.Empty;

    private int? _editandoId;

    partial void OnSeleccionadoChanged(Plan? value)
    {
        if (value is null) return;
        _editandoId = value.Id;
        Titulo = "Editar plan";
        Nombre = value.Nombre;
        Frecuencia = value.Frecuencia;
        NumeroCuotas = value.NumeroCuotas;
        DiasEntreCuotas = value.DiasEntreCuotas;
        InteresTotal = value.InteresTotalPorcentaje;
        MultiploRedondeo = value.MultiploRedondeo;
        Activo = value.Activo;
    }

    // Al cambiar la frecuencia se proponen sus días habituales; se pueden cambiar después.
    partial void OnFrecuenciaChanged(FrecuenciaPago value) => DiasEntreCuotas = Plan.DiasPorDefecto(value);

    partial void OnNumeroCuotasChanged(int value) => CalcularEjemplo();
    partial void OnDiasEntreCuotasChanged(int value) => CalcularEjemplo();
    partial void OnInteresTotalChanged(decimal value) => CalcularEjemplo();
    partial void OnMultiploRedondeoChanged(int value) => CalcularEjemplo();
    partial void OnMontoEjemploChanged(decimal value) => CalcularEjemplo();

    private void Cargar()
    {
        using var db = _crearDb();
        Lista.Clear();
        foreach (var plan in db.Planes.AsNoTracking().OrderByDescending(p => p.Activo).ThenBy(p => p.Nombre))
            Lista.Add(plan);
    }

    [RelayCommand]
    private void Nuevo()
    {
        _editandoId = null;
        Seleccionado = null;
        Titulo = "Nuevo plan";
        Nombre = string.Empty;
        Frecuencia = FrecuenciaPago.Semanal;
        DiasEntreCuotas = 7;
        NumeroCuotas = 6;
        InteresTotal = 60m;
        MultiploRedondeo = 10;
        Activo = true;
    }

    private void CalcularEjemplo()
    {
        Ejemplo.Clear();
        try
        {
            var cuotas = CalculadoraCuotas.Generar(MontoEjemplo, InteresTotal, NumeroCuotas, DiasEntreCuotas, MultiploRedondeo, DateOnly.FromDateTime(DateTime.Today));
            foreach (var c in cuotas) Ejemplo.Add(CuotaFila.Desde(c));
            ResumenEjemplo = $"Devuelve {cuotas.Sum(c => c.Monto):N2}: {Formato.Cuotas(cuotas)}";
        }
        catch (ArgumentException ex)
        {
            ResumenEjemplo = ex.Message;
        }
    }

    [RelayCommand]
    private void Guardar()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            MessageBox.Show("Ponle un nombre al plan.", "Planes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (NumeroCuotas < 1 || DiasEntreCuotas < 1 || InteresTotal < 0 || MultiploRedondeo < 0)
        {
            MessageBox.Show("Revisa los valores: al menos 1 cuota y 1 día entre cuotas, sin números negativos.", "Planes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = _crearDb();
        var nombre = Nombre.Trim();
        if (db.Planes.Any(p => p.Nombre == nombre && p.Id != _editandoId))
        {
            MessageBox.Show($"Ya existe un plan llamado «{nombre}».", "Planes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var plan = _editandoId is int id ? db.Planes.Find(id)! : db.Planes.Add(new Plan()).Entity;
        plan.Nombre = nombre;
        plan.Frecuencia = Frecuencia;
        plan.NumeroCuotas = NumeroCuotas;
        plan.DiasEntreCuotas = DiasEntreCuotas;
        plan.InteresTotalPorcentaje = InteresTotal;
        plan.MultiploRedondeo = MultiploRedondeo;
        plan.Activo = Activo;
        db.SaveChanges();

        Cargar();
        Nuevo();
        PlanGuardado?.Invoke(this, EventArgs.Empty);
    }
}
