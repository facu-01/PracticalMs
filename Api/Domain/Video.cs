using System;
using JasperFx.Events;

namespace Api.Domain;


public record VideoUploaded(string Title);
public record VideoViewed(
    Guid VideoId
);


public class Video
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public DateTime CreatedAt { get; set; }

    // Método Create para inicializar el aggregate con el primer evento
    public static Video Create(VideoUploaded @event)
    {
        return new Video
        {
            Title = @event.Title,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Método Apply para procesar eventos subsecuentes
    public void Apply(VideoUploaded @event)
    {
        Title = @event.Title;
    }

    // public void Apply(VideoViewed @event)
    // {
    //     // Video viewed event - no cambia el estado del video
    // }
}
