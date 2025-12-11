namespace Api.Domain;

public record UserRegistered(
    Guid UserId,
    string Email,
    string PasswordHash);

public record UserVideoViewed(Guid UserId, Guid VideoId);

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public int VideosWatched { get; set; } = 0;

    public static User Create(UserRegistered @event)
    {
        return new User
        {
            Email = @event.Email,
            PasswordHash = @event.PasswordHash
        };
    }

    public void Apply(UserVideoViewed _)
    {
        VideosWatched++;
    }

}