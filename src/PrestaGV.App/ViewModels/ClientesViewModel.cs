using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using PrestaGV.Core.Modelos;
using PrestaGV.Data;

namespace PrestaGV.App.ViewModels;

public partial class ClientesViewModel : ObservableObject
{
    private readonly Func<PrestaGVDbContext> _crearDb;

    public ClientesViewModel(Func<PrestaGVDbContext> crearDb)
    {
        _crearDb = crearDb;
        Cargar();
    }

    public event EventHandler? ClienteGuardado;

    public ObservableCollection<Cliente> Lista { get; } = [];

    [ObservableProperty] private string _busqueda = string.Empty;
    [ObservableProperty] private string _nombres = string.Empty;
    [ObservableProperty] private string _apellidos = string.Empty;
    [ObservableProperty] private string _documento = string.Empty;
    [ObservableProperty] private string _telefono = string.Empty;
    [ObservableProperty] private string _email = string.Empty;
    [ObservableProperty] private string _direccion = string.Empty;

    partial void OnBusquedaChanged(string value) => Cargar();

    private void Cargar()
    {
        using var db = _crearDb();
        var consulta = db.Clientes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(Busqueda))
        {
            var texto = Busqueda.Trim();
            consulta = consulta.Where(c => c.Nombres.Contains(texto) || c.Apellidos.Contains(texto) || c.Documento.Contains(texto));
        }

        Lista.Clear();
        foreach (var cliente in consulta.OrderBy(c => c.Apellidos).ThenBy(c => c.Nombres))
            Lista.Add(cliente);
    }

    [RelayCommand]
    private void Guardar()
    {
        if (string.IsNullOrWhiteSpace(Nombres) || string.IsNullOrWhiteSpace(Apellidos) || string.IsNullOrWhiteSpace(Documento))
        {
            MessageBox.Show("Nombres, apellidos y documento son obligatorios.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        using var db = _crearDb();
        var documento = Documento.Trim();
        if (db.Clientes.Any(c => c.Documento == documento))
        {
            MessageBox.Show($"Ya existe un cliente con el documento {documento}.", "Clientes", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        db.Clientes.Add(new Cliente
        {
            Nombres = Nombres.Trim(),
            Apellidos = Apellidos.Trim(),
            Documento = documento,
            Telefono = Vacio(Telefono),
            Email = Vacio(Email),
            Direccion = Vacio(Direccion),
        });
        db.SaveChanges();

        Nombres = Apellidos = Documento = Telefono = Email = Direccion = string.Empty;
        Cargar();
        ClienteGuardado?.Invoke(this, EventArgs.Empty);
    }

    private static string? Vacio(string texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}
