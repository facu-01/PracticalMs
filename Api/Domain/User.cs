namespace Api.Domain;

public record UserRegistered(string Name);

public record UserVideoViewed(Guid UserId, Guid VideoId);

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;

    public int VideosWatched { get; set; } = 0;

    public static User Create(UserRegistered @event)
    {
        return new User
        {
            Name = @event.Name
        };
    }

    public void Apply(UserVideoViewed _)
    {
        VideosWatched++;
    }

}