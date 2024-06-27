namespace Conduit.API;

public record CommentDto(string Body)
{
    public string Id { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; } 
    public ProfileDto Author { get; init; }
};
