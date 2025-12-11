using System;
using Api.Rendering;
using Api.Rendering.Layouts;
using JasperFx.Descriptors;
using Microsoft.AspNetCore.Mvc;
using Wolverine.Http;

namespace Api.Features.RegisterUsers;

public record RegisterCommand(string Email, string Password);


public class Routes
{
    [WolverineGet("/register")]
    public static async Task<IResult> Get(
        [FromHeader(Name = "HX-Request")] string? hxRequest,
        ComponentRenderer renderer
    )
    {

        var html = await renderer.RenderAsync<RegisterForm>(isPartial: !string.IsNullOrEmpty(hxRequest));

        return Results.Content(html, "text/html");
    }

    [WolverinePost("/register")]
    public static async Task<IResult> Post(
        RegisterCommand command,
        ComponentRenderer renderer
    )
    {

        return Results.Content("test", "plain/text");
    }

}
