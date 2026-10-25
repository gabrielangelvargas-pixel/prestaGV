using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PrestaGV.App.Servicios;
using PrestaGV.Data;

namespace PrestaGV.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ServicioActualizaciones _actualizaciones;

    public MainViewModel(Func<PrestaGVDbContext> crearDb, ServicioActualizaciones actualizaciones)
    {
        _actualizaciones = actualizaciones;
        Clientes = new ClientesViewModel(crearDb);
        Planes = new PlanesViewModel(crearDb);
        Prestamos = new PrestamosViewModel(crearDb);
        Clientes.ClienteGuardado += (_, _) => Prestamos.CargarClientes();
        Planes.PlanGuardado += (_, _) => Prestamos.CargarPlanes();

        EstadoActualizacion = actualizaciones.EstaInstalada ? string.Empty : "Modo desarrollo: actualizaciones desactivadas";
        _ = BuscarActualizacionesAsync();
    }

    public ClientesViewModel Clientes { get; }
    public PlanesViewModel Planes { get; }
    public PrestamosViewModel Prestamos { get; }

    public string Version => $"PrestaGV {_actualizaciones.VersionActual}";

    [ObservableProperty]
    private string _estadoActualizacion;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReiniciarParaActualizarCommand))]
    private bool _actualizacionLista;

    [RelayCommand]
    private async Task BuscarActualizacionesAsync()
    {
        if (!_actualizaciones.EstaInstalada) return;

        try
        {
            EstadoActualizacion = "Buscando actualizaciones...";
            var nueva = await _actualizaciones.BuscarYDescargarAsync();
            ActualizacionLista = nueva is not null;
            EstadoActualizacion = nueva is null ? "La aplicación está al día" : $"Versión {nueva} lista para instalar";
        }
        catch (Exception ex)
        {
            EstadoActualizacion = $"No se pudo buscar actualizaciones: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(ActualizacionLista))]
    private void ReiniciarParaActualizar() => _actualizaciones.AplicarYReiniciar();
}
