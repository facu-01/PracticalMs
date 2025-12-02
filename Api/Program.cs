using Api.Middlewares;
using Api.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Wolverine;
using Wolverine.Http;
using Marten;
using Wolverine.Marten;
using JasperFx;
using JasperFx.Events.Daemon;
using Api.Database;
using Api.Domain;
using JasperFx.Events.Projections;
using Marten.Events.Projections;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Components
builder.Services.AddRazorComponents();
builder.Services.AddScoped<HtmlRenderer>();
builder.Services.AddScoped<ComponentRenderer>();

// OpenAPI (ASP.NET Core 9)
builder.Services.AddOpenApi();


// Add Marten and Wolverine
builder.Services.AddMarten(
opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("Db");
    opts.Connection(connectionString!);
    opts.DatabaseSchemaName = "videos";

    // Registrar el aggregate Video para event sourcing
    opts.Projections.Snapshot<Video>(SnapshotLifecycle.Inline);

    // Registrar proyecciones
    opts.Projections.Add<GlobalVideoCounterProjection>(ProjectionLifecycle.Async);

    opts.AutoCreateSchemaObjects = AutoCreate.All;
})
.UseLightweightSessions()
.InitializeWith(new InitialData(InitialDatasets.VideosUploaded))
.AddAsyncDaemon(DaemonMode.Solo)
.IntegrateWithWolverine();

builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions();
});


builder.Services.AddWolverineHttp();

var app = builder.Build();


app.UseStaticFiles();



app.ApplyMigration();

app.MapWolverineEndpoints(opts =>
{
    opts.AddMiddleware<CorrelationMiddleware>();
});

app.UseMiddleware<ExceptionHandlerMiddleware>();


// reemplaza al app.Run(), dando posibilidad de ejecutar comandos JasperFx.
return await app.RunJasperFxCommands(args);


