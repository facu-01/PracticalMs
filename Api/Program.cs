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
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);


// OpenAPI (ASP.NET Core 9)
builder.Services.AddOpenApi();

builder.Services.AddScoped<ComponentRenderer>();


// Add Marten and Wolverine
builder.Services.AddMarten(
opts =>
{
    var connectionString = builder.Configuration.GetConnectionString("Db");
    opts.Connection(connectionString!);
    opts.DatabaseSchemaName = "videos";

    // Registramos los aggregates como snapshots inline
    opts.Projections.Snapshot<User>(SnapshotLifecycle.Inline);
    opts.Projections.Snapshot<Video>(SnapshotLifecycle.Inline);

    // Registrar proyecciones
    opts.Projections.Add<GlobalVideoCounterProjection>(ProjectionLifecycle.Async);

    opts.AutoCreateSchemaObjects = AutoCreate.All;

    opts.Events.UseIdentityMapForAggregates = true;
})
.UseLightweightSessions()
.InitializeWith(new InitialData(InitialDatasets.VideosUploaded, InitialDatasets.UsersRegistered))
.AddAsyncDaemon(DaemonMode.Solo)
.IntegrateWithWolverine();

builder.Host.UseWolverine(opts =>
{
    opts.Policies.AutoApplyTransactions();
});


builder.Services.AddWolverineHttp();


builder.Services.AddValidatorsFromAssemblyContaining<Program>(); // o cualquier tipo del ensamblado


var app = builder.Build();


app.UseStaticFiles();



// Enable OpenAPI/Swagger and ReDoc UI
// Expose OpenAPI document
app.MapOpenApi();

// ReDoc UI using Swashbuckle.ReDoc, pointing to Microsoft.AspNetCore.OpenApi document
app.UseReDoc(options =>
{
    options.DocumentTitle = "Open API - ReDoc";
    options.SpecUrl("/openapi/v1.json");
    options.RoutePrefix = "docs"; // change relative path to the UI
});


app.MapWolverineEndpoints(opts =>
{
    opts.AddMiddleware<CorrelationMiddleware>();
    opts.AddMiddleware<HtmxMiddleware>();
});

app.UseMiddleware<ExceptionHandlerMiddleware>();


// reemplaza al app.Run(), dando posibilidad de ejecutar comandos JasperFx.
return await app.RunJasperFxCommands(args);


