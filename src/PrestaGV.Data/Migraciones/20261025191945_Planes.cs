using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PrestaGV.Data.Migraciones
{
    /// <inheritdoc />
    public partial class Planes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TipoAmortizacion",
                table: "Prestamos");

            migrationBuilder.RenameColumn(
                name: "TasaInteresMensual",
                table: "Prestamos",
                newName: "InteresTotalPorcentaje");

            migrationBuilder.AddColumn<int>(
                name: "DiasEntreCuotas",
                table: "Prestamos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MultiploRedondeo",
                table: "Prestamos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanId",
                table: "Prestamos",
                type: "INTEGER",
                nullable: true);

            // Préstamos creados con la versión 0.1.0: la tasa era mensual, así que se recalcula
            // el interés total a partir de sus cuotas y se fijan los días según la frecuencia.
            migrationBuilder.Sql("""
                UPDATE Prestamos SET
                    DiasEntreCuotas = CASE Frecuencia WHEN 'Semanal' THEN 7 WHEN 'Quincenal' THEN 15 ELSE 30 END,
                    InteresTotalPorcentaje = ROUND(
                        COALESCE((SELECT SUM(CAST(c.Interes AS REAL)) FROM Cuotas c WHERE c.PrestamoId = Prestamos.Id), 0)
                        * 100.0 / CAST(Monto AS REAL), 2);
                """);

            migrationBuilder.CreateTable(
                name: "Planes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Frecuencia = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NumeroCuotas = table.Column<int>(type: "INTEGER", nullable: false),
                    DiasEntreCuotas = table.Column<int>(type: "INTEGER", nullable: false),
                    InteresTotalPorcentaje = table.Column<decimal>(type: "TEXT", nullable: false),
                    MultiploRedondeo = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Planes",
                columns: new[] { "Id", "Activo", "DiasEntreCuotas", "Frecuencia", "InteresTotalPorcentaje", "MultiploRedondeo", "Nombre", "NumeroCuotas" },
                values: new object[] { 1, true, 7, "Semanal", 60m, 10, "Semanal 6 cuotas 60 %", 6 });

            migrationBuilder.CreateIndex(
                name: "IX_Prestamos_PlanId",
                table: "Prestamos",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Planes_Nombre",
                table: "Planes",
                column: "Nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Prestamos_Planes_PlanId",
                table: "Prestamos",
                column: "PlanId",
                principalTable: "Planes",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prestamos_Planes_PlanId",
                table: "Prestamos");

            migrationBuilder.DropTable(
                name: "Planes");

            migrationBuilder.DropIndex(
                name: "IX_Prestamos_PlanId",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "DiasEntreCuotas",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "MultiploRedondeo",
                table: "Prestamos");

            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "Prestamos");

            migrationBuilder.RenameColumn(
                name: "InteresTotalPorcentaje",
                table: "Prestamos",
                newName: "TasaInteresMensual");

            migrationBuilder.AddColumn<string>(
                name: "TipoAmortizacion",
                table: "Prestamos",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
