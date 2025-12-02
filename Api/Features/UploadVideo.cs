using System;
using Api.Domain;
using Wolverine.Http;
using Wolverine.Marten;

namespace Api.Features;


public record UploadVideoCommand(
    string Title
);

public static class UploadVideoEndpoint
{
    [WolverinePost("/videos")]
    public static (CreationResponse<Guid>, IStartStream) Post(UploadVideoCommand command)
    {
        var uploadedVideo = new VideoUploaded(command.Title);

        var start = MartenOps.StartStream<Video>(uploadedVideo);

        var response = new CreationResponse<Guid>("/videos/" + start.StreamId, start.StreamId);

        return (response, start);

    }

}
