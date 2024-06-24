namespace Conduit.API.Dso;

public record CommentDso(string Id, string Body, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, ProfileDso Author);
