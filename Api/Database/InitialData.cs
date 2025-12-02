using System;
using Api.Domain;
using Marten;
using Marten.Schema;

namespace Api.Database;

public class InitialData : IInitialData
{
    private readonly (Guid, VideoUploaded)[] _videos;

    public InitialData((Guid, VideoUploaded)[] videos)
    {
        _videos = videos;
    }

    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {

        try
        {
            await using var session = store.LightweightSession();

            foreach (var (id, videoUploaded) in _videos)
            {
                session.Events.StartStream<Video>(id, videoUploaded);
            }

            await session.SaveChangesAsync();
        }
        catch (Marten.Exceptions.ExistingStreamIdCollisionException)
        {


        }
    }
}

public static class InitialDatasets
{
    public static readonly (Guid, VideoUploaded)[] VideosUploaded =
    {
       (Guid.Parse("2219b6f7-7883-4629-95d5-1a8a6c74b244"), new VideoUploaded("Test 1")),
       (Guid.Parse("642a3e95-5875-498e-8ca0-93639ddfebcd"), new VideoUploaded("Test 2")),
    };
}