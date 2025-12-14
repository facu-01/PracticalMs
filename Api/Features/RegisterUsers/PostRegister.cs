using System.Net;
using Api.Domain;
using Api.Middlewares;
using Api.Rendering;
using FluentValidation;
using JasperFx.CodeGeneration;
using JasperFx.CodeGeneration.Frames;
using Marten;
using Marten.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Attributes;
using Wolverine.ErrorHandling;
using Wolverine.Http;
using Wolverine.Marten;
using Wolverine.Runtime.Handlers;

namespace Api.Features.RegisterUsers;

public record RegisterCommand(string Email, string Password);


public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es requerido.")
            .EmailAddress().WithMessage("El email no es válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es requerida.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.");
    }
}

public class CatchConflictFrame : AsyncFrame
{
    public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
    {
        // try {
        writer.Write("BLOCK:try");
        Next?.GenerateCode(method, writer);
        writer.FinishBlock(); // cierra el try

        // catch (DocumentAlreadyExistsException e) {
        writer.Write($"BLOCK:catch({typeof(DocumentAlreadyExistsException).FullName} e)");
        writer.Write($"await {typeof(CatchConflictFrame).FullName}.{nameof(RespondWithProblemDetails)}(e, httpContext);");
        writer.Write("return;");
        writer.FinishBlock(); // cierra el catch

    }

    public static Task RespondWithProblemDetails(Exception ex, HttpContext context)
    {
        var detail = ex switch
        {
            DocumentAlreadyExistsException dae => dae.Message,
            Npgsql.PostgresException pg when pg.SqlState == "23505" => pg.MessageText,
            _ => ex.Message
        };

        var problems = new ProblemDetails
        {
            Title = "Conflicto de datos",
            Detail = detail,
            Status = StatusCodes.Status409Conflict,
            Instance = context.Request.Path
        };

        return Results.Problem(problems).ExecuteAsync(context);
    }
}


public static class PostRegisterEndpoint
{

    public static async Task<IResult> ValidateAsync(
        RegisterCommand command,
        ComponentRenderer renderer,
        HttpContext context,
        IValidator<RegisterCommand> validator,
        IDocumentSession session
    )
    {

        var result = await validator.ValidateAsync(command);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName.ToLowerInvariant())
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToList());

            var html = await renderer.RenderAsync<RegisterForm>(
            builder
            =>
            {
                builder.Add(c => c.Errors, errors);
                builder.Add(c => c.Email, command.Email);
                builder.Add(c => c.Password, command.Password);
            },
            isPartial: context.IsHtmx());
            return Results.Content(html, "text/html", statusCode: (int)HttpStatusCode.BadRequest);
        }


        // verificamos si el email ya está registrado
        var existingUser = await session.Query<User>()
            .FirstOrDefaultAsync(u => u.Email == command.Email.Trim());

        if (existingUser != null)
        {
            var errors = new Dictionary<string, List<string>>
            {
                { "email", new List<string> { "El email ya está registrado." } }
            };

            var html = await renderer.RenderAsync<RegisterForm>(
            builder
            =>
            {
                builder.Add(c => c.Errors, errors);
                builder.Add(c => c.Email, command.Email);
                builder.Add(c => c.Password, command.Password);
            },
            isPartial: context.IsHtmx());
            return Results.Content(html, "text/html", statusCode: (int)HttpStatusCode.BadRequest);
        }

        return WolverineContinue.Result();
    }


    public static void Configure(HttpChain chain)
    {
        chain.Middleware.Insert(0, new CatchConflictFrame());

        // Documenta OpenAPI: devuelve problem+json 409
        chain.Metadata.ProducesProblem(409);
    }

    [WolverinePost("/register")]
    public static async Task<(IResult, IStartStream)> PostRegister(
        RegisterCommand command
    )
    {
        var clearEmail = command.Email.Trim();

        // var passwordHasher = new PasswordHasher<object>();

        // var passwordHash = passwordHasher.HashPassword(null, command.Password);

        var userRegistered = new UserRegistered(
            Guid.NewGuid(),
            clearEmail,
            // passwordHash,
            command.Password
        );

        var startStream = MartenOps.StartStream<User>(userRegistered);


        return (
            Results.Redirect("/register/registration-complete"),
            startStream
        );
    }
}