using System;
using Api.Domain;
using Marten;
using Marten.Schema;

namespace Api.Database;

public class InitialData : IInitialData
{
    private readonly (Guid, VideoUploaded)[] _videos;
    private readonly (Guid, UserRegistered)[] _users;


    public InitialData((Guid, VideoUploaded)[] videos, (Guid, UserRegistered)[] users)
    {
        _videos = videos;
        _users = users;
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

            foreach (var (id, userRegistered) in _users)
            {
                session.Events.StartStream<User>(id, userRegistered);
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

    public static readonly (Guid, UserRegistered)[] UsersRegistered =
    {
       (Guid.Parse("a2f06238-05bb-4587-9d3a-2976c5c0d9a7"), new UserRegistered(Guid.Parse("a2f06238-05bb-4587-9d3a-2976c5c0d9a7"), "initialuser1@example.com", "passwordhash1")),
       (Guid.Parse("43006900-eb16-4afa-9c01-f85195d0f9fb"), new UserRegistered(Guid.Parse("43006900-eb16-4afa-9c01-f85195d0f9fb"), "initialuser2@example.com", "passwordhash2")),
    };



}