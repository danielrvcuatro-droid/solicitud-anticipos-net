using System.Text.Json.Serialization;
using SolicitudAnticipos.Api.Aplicacion;
using SolicitudAnticipos.Api.Configuracion;
using SolicitudAnticipos.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(opciones => opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DbContext + repositorios contra Supabase (ver ConnectionStrings:Supabase más abajo).
builder.Services.AddInfrastructure(builder.Configuration);
// Subida de adjuntos a SharePoint vía Microsoft Graph (ver la sección SharePoint más abajo).
builder.Services.AddSharePoint(builder.Configuration);
builder.Services.Configure<ConfiguracionAprobacion>(builder.Configuration.GetSection(ConfiguracionAprobacion.Seccion));
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IIngestaSolicitudService, IngestaSolicitudService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Necesario para que WebApplicationFactory<Program> (pruebas de integración) pueda referenciar este entry point.
public partial class Program;
