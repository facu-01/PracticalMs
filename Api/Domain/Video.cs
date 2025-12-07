namespace Api.Domain;


public record VideoUploaded(string Title);
public record VideoViewed(
    Guid VideoId,
    Guid UserId
);

public class Video
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    public static Video Create(VideoUploaded @event)
    {
        return new Video
        {
            Title = @event.Title,
            CreatedAt = DateTime.UtcNow
        };
    }
    public void Apply(VideoUploaded @event)
    {
        Title = @event.Title;
    }

    public void Apply(VideoViewed _)
    {


    }
}
