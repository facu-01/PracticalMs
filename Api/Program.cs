using Api.Database;
using Api.Middlewares;
using Wolverine;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddWolverineHttp();
builder.Host.UseWolverine();

builder.Services.AddDatabase(builder.Configuration);

var app = builder.Build();


app.ApplyMigration();
app.UseStaticFiles();
app.MapFallbackToFile("/index.html");

app.MapWolverineEndpoints(opts =>
{
    opts.AddMiddleware<CorrelationMiddleware>();
});

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.Run();
