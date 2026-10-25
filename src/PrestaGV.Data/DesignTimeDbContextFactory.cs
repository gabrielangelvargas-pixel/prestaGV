using Microsoft.EntityFrameworkCore.Design;

namespace PrestaGV.Data;

/// <summary>Usado por <c>dotnet ef</c> para generar migraciones.</summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<PrestaGVDbContext>
{
    public PrestaGVDbContext CreateDbContext(string[] args) => new(BaseDeDatos.Opciones("prestagv-design.db"));
}
