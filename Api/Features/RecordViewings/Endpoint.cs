using System;
using Api.Domain;
using Api.Rendering;
using Api.Rendering.Layouts;
using Marten;
using Marten.Events;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace Api.Features.RecordViewings;

public record RecordViewingCommand(Guid VideoId, Guid UserId);

public static class Endpoint
{

    [WolverinePost("/record-viewing")]
    [EmptyResponse]
    public static (VideoViewed, UserVideoViewed) Post(
        RecordViewingCommand command,
        [WriteAggregate(nameof(RecordViewingCommand.VideoId))] Video video,
        [WriteAggregate(nameof(RecordViewingCommand.UserId))] User user
    )
    {

        return (new VideoViewed(video.Id, user.Id), new UserVideoViewed(user.Id, video.Id));

    }

}
