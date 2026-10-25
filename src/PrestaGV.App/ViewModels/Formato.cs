using PrestaGV.Core.Modelos;

namespace PrestaGV.App.ViewModels;

public static class Formato
{
    /// <summary>Resume las cuotas agrupando importes iguales consecutivos: "5 x 2.670,00 y 1 x 2.650,00".</summary>
    public static string Cuotas(IEnumerable<Cuota> cuotas)
    {
        var grupos = new List<(int Cantidad, decimal Monto)>();
        foreach (var c in cuotas)
        {
            if (grupos.Count > 0 && grupos[^1].Monto == c.Monto)
                grupos[^1] = (grupos[^1].Cantidad + 1, c.Monto);
            else
                grupos.Add((1, c.Monto));
        }

        var partes = grupos.Select(g => $"{g.Cantidad} x {g.Monto:N2}").ToList();
        return partes.Count <= 1 ? string.Join("", partes) : $"{string.Join(", ", partes[..^1])} y {partes[^1]}";
    }
}
