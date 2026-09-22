using Microsoft.Extensions.Options;
using SolicitudAnticipos.Infrastructure.SharePoint;

// Herramienta de línea de comandos para probar, de forma aislada, que la subida a SharePoint vía
// Microsoft Graph funciona con las credenciales reales del App Registration — sin necesitar
// Supabase, matriz de aprobación, ni levantar la Api completa.
//
// Uso:
//   dotnet run --project tools/SolicitudAnticipos.Herramientas.ProbarSharePoint -- "C:\ruta\a\archivo.pdf"
//
// Lee la configuración de variables de entorno (para no duplicar los secretos que ya guardaste
// con dotnet user-secrets en la Api). Antes de correrlo, en la misma terminal:
//   $env:SHAREPOINT_TENANT_ID = "..."
//   $env:SHAREPOINT_CLIENT_ID = "..."
//   $env:SHAREPOINT_CLIENT_SECRET = "..."
//   $env:SHAREPOINT_SITE_ID = "..."

if (args.Length != 1)
{
    Console.Error.WriteLine("Uso: dotnet run --project tools/SolicitudAnticipos.Herramientas.ProbarSharePoint -- \"ruta\\al\\archivo.pdf\"");
    return 1;
}

var rutaArchivo = args[0];
if (!File.Exists(rutaArchivo))
{
    Console.Error.WriteLine($"No se encontró el archivo: {rutaArchivo}");
    return 1;
}

string LeerVariableRequerida(string nombre)
{
    var valor = Environment.GetEnvironmentVariable(nombre);
    if (string.IsNullOrWhiteSpace(valor))
    {
        throw new InvalidOperationException($"Falta la variable de entorno {nombre}. Revisa el comentario al inicio de Program.cs.");
    }

    return valor;
}

var configuracion = new ConfiguracionSharePoint
{
    TenantId = LeerVariableRequerida("SHAREPOINT_TENANT_ID"),
    ClientId = LeerVariableRequerida("SHAREPOINT_CLIENT_ID"),
    ClientSecret = LeerVariableRequerida("SHAREPOINT_CLIENT_SECRET"),
    SiteId = LeerVariableRequerida("SHAREPOINT_SITE_ID"),
    CarpetaDestino = Environment.GetEnvironmentVariable("SHAREPOINT_CARPETA_DESTINO") ?? "SolicitudesAnticipos",
};

var almacenamiento = new SharePointAlmacenamientoAdjuntos(Options.Create(configuracion));

var nombreArchivo = Path.GetFileName(rutaArchivo);
var contenido = await File.ReadAllBytesAsync(rutaArchivo);
var tipoContenido = Path.GetExtension(rutaArchivo).Equals(".pdf", StringComparison.OrdinalIgnoreCase)
    ? "application/pdf"
    : "application/octet-stream";

Console.WriteLine($"Subiendo '{nombreArchivo}' ({contenido.Length} bytes) al sitio configurado...");

try
{
    var carpetaDePrueba = $"pruebas-manuales/{DateTime.UtcNow:yyyyMMdd-HHmmss}";
    var url = await almacenamiento.SubirAsync(carpetaDePrueba, nombreArchivo, contenido, tipoContenido);
    Console.WriteLine();
    Console.WriteLine("¡Listo! El archivo quedó subido en:");
    Console.WriteLine(url);
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine();
    Console.Error.WriteLine("Falló la subida. Detalle del error:");
    Console.Error.WriteLine(ex);
    return 1;
}
