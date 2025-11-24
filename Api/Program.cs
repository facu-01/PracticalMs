using Api.Middlewares;
using Wolverine;
using Wolverine.Http;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddWolverineHttp();
builder.Host.UseWolverine();



var app = builder.Build();



app.UseStaticFiles();
app.MapFallbackToFile("/index.html");

app.MapWolverineEndpoints(opts =>
{
    opts.AddMiddleware<CorrelationMiddleware>();
});

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.Run();
