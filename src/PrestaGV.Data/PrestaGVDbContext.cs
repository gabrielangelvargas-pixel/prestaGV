using Microsoft.EntityFrameworkCore;
using PrestaGV.Core.Modelos;

namespace PrestaGV.Data;

public class PrestaGVDbContext(DbContextOptions<PrestaGVDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<Cuota> Cuotas => Set<Cuota>();
    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(e =>
        {
            e.Property(c => c.Nombres).HasMaxLength(100).IsRequired();
            e.Property(c => c.Apellidos).HasMaxLength(100).IsRequired();
            e.Property(c => c.Documento).HasMaxLength(30).IsRequired();
            e.HasIndex(c => c.Documento).IsUnique();
            e.Ignore(c => c.NombreCompleto);
        });

        modelBuilder.Entity<Prestamo>(e =>
        {
            e.HasOne(p => p.Cliente).WithMany(c => c.Prestamos).HasForeignKey(p => p.ClienteId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(p => p.Cuotas).WithOne(c => c.Prestamo).HasForeignKey(c => c.PrestamoId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(p => p.Pagos).WithOne(p => p.Prestamo).HasForeignKey(p => p.PrestamoId).OnDelete(DeleteBehavior.Cascade);
            e.Property(p => p.Frecuencia).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.TipoAmortizacion).HasConversion<string>().HasMaxLength(20);
            e.Property(p => p.Estado).HasConversion<string>().HasMaxLength(20);
            e.Ignore(p => p.TotalAPagar);
            e.Ignore(p => p.TotalPagado);
            e.Ignore(p => p.SaldoPendiente);
        });

        modelBuilder.Entity<Cuota>(e =>
        {
            e.HasIndex(c => new { c.PrestamoId, c.Numero }).IsUnique();
            e.Ignore(c => c.Monto);
            e.Ignore(c => c.Pendiente);
        });
    }
}
