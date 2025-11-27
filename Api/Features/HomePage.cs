using System;
using RazorSlices;
using Wolverine.Http;

namespace Api.Features;

public static class HomePageEndpoint
{
    [WolverineGet("/")]
    public static IResult Get()
    {
        return Results.Extensions.RazorSlice<Slices.Home, string>("Test User");
    }

}
