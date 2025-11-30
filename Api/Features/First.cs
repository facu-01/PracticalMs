using System;
using Wolverine.Http;

namespace Api.Features;



public static class FirstEndpoint
{
    [WolverineGet("/first")]
    public static string Get()
    {
        return "First endpoint response at " + DateTime.Now;
    }

}