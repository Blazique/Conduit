namespace Conduit.API;

public record CommentDto(string Id, string Body, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, ProfileDto Author);
