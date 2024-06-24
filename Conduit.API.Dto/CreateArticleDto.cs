namespace Conduit.API;

public record CreateArticleDto(string Title, string Description, string Body, List<string>? TagList);
