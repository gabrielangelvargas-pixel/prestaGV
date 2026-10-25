# PrestaGV

Aplicación de escritorio para Windows para gestionar préstamos personales: clientes, préstamos,
calendario de cuotas y registro de pagos. Se distribuye con un instalador que se actualiza solo.

## Stack

| Parte | Tecnología |
| --- | --- |
| Aplicación | .NET 10 (LTS) + WPF, patrón MVVM con CommunityToolkit.Mvvm |
| Datos | SQLite local con Entity Framework Core 10 y migraciones |
| Instalador y actualizaciones | [Velopack](https://velopack.io), publicado en GitHub Releases |
| CI/CD | GitHub Actions |

## Estructura

```
src/
  PrestaGV.Core/    Modelo (Cliente, Prestamo, Cuota, Pago) y reglas de negocio, sin dependencias
  PrestaGV.Data/    DbContext de EF Core, migraciones y ubicación de la base de datos
  PrestaGV.App/     Aplicación WPF (ventanas, view models y servicio de actualizaciones)
tests/
  PrestaGV.Core.Tests/  Pruebas de cálculos, pagos y base de datos (xUnit)
.github/workflows/
  ci.yml            Compila y prueba en cada push y pull request
  release.yml       Genera el instalador y publica la versión al crear una etiqueta v*
```

## Reglas de negocio

- **Tasa de interés**: se introduce como porcentaje **mensual**. Para pagos semanales o quincenales
  se convierte al período (5 % mensual = 2,5 % quincenal).
- **Tipos de amortización**:
  - *Francés*: cuota fija; el interés se calcula sobre el saldo pendiente.
  - *Interés simple*: el interés total es `monto × tasa × cuotas` y se reparte en partes iguales.
- **Redondeo**: todo se redondea a 2 decimales; la última cuota absorbe la diferencia.
- **Pagos**: se aplican a la cuota pendiente más antigua. Un pago no puede superar el saldo.
  Cuando el saldo llega a cero el préstamo pasa a *Saldado*.
- **Base de datos**: `%LocalAppData%\PrestaGV\prestagv.db`, fuera de la carpeta de instalación
  para que las actualizaciones no la borren. Se crea y migra sola al abrir la aplicación.

## Desarrollo

Requisitos: [SDK de .NET 10](https://dotnet.microsoft.com/download) y Visual Studio 2026,
Rider o VS Code con C# Dev Kit.

```powershell
dotnet tool restore          # instala dotnet-ef
dotnet build
dotnet test
dotnet run --project src/PrestaGV.App
```

Al ejecutar desde el código fuente, la búsqueda de actualizaciones está desactivada
(solo funciona en la versión instalada).

### Cambiar el modelo de datos

```powershell
dotnet ef migrations add NombreDelCambio --project src/PrestaGV.Data --output-dir Migraciones
```

La CI falla si el modelo cambia y falta la migración.

## Publicar una versión

1. En GitHub ve a **Actions > Release > Run workflow** y escribe la versión (semver, por ejemplo
   `0.2.0`). También se puede lanzar subiendo una etiqueta:
   ```powershell
   git tag v0.2.0
   git push origin v0.2.0
   ```
2. El workflow **Release** compila, ejecuta las pruebas, empaqueta con Velopack y publica en
   GitHub Releases:
   - `PrestaGV-win-Setup.exe`: el instalador que se enlaza desde el sitio web.
   - `*-full.nupkg`, `*-delta.nupkg` y `releases.win.json`: lo que usa la app para actualizarse.
3. Las copias instaladas detectan la versión nueva al abrirse; el usuario pulsa
   **Reiniciar y actualizar** en la barra inferior.

> La versión de la herramienta `vpk` en `release.yml` debe coincidir con la del paquete
> `Velopack` en `PrestaGV.App.csproj`.

## Próximos pasos

- Periodo de prueba y licencias (Keygen.sh, LemonSqueezy o API propia).
- Firma de código con Azure Trusted Signing para evitar el aviso de SmartScreen.
- Sitio web de descarga (Astro en Vercel o Cloudflare Pages) enlazando al último `Setup.exe`.
- Edición de clientes, mora y reportes.
