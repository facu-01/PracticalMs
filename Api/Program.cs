using Api.Database;
using Api.Middlewares;
using Api.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Wolverine;
using Wolverine.Http;
using Api.Features;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Components
builder.Services.AddRazorComponents();
builder.Services.AddScoped<HtmlRenderer>();
builder.Services.AddScoped<ComponentRenderer>();

builder.Services.AddWolverineHttp();
builder.Host.UseWolverine();

builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();


app.UseStaticFiles();

app.ApplyMigration();

app.MapWolverineEndpoints(opts =>
{
    opts.AddMiddleware<CorrelationMiddleware>();
});

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.Run();
