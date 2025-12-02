using System;
using Api.Domain;
using Api.Rendering;
using Api.Rendering.Layouts;
using Wolverine.Http;
using Wolverine.Http.Marten;

namespace Api.Features.RecordViewings;


public static class Endpoint
{
    [EmptyResponse]
    [WolverinePost("/record-viewing/{videoId:guid}")]
    public static VideoViewed Post(
        [Aggregate("videoId")] Video video
    )
    {

        return new VideoViewed(video.Id);
    }

}
