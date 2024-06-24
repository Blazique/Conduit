namespace Conduit.API;

public record ArticleFeedDto(int ArticlesCount, List<ArticleDto> Articles);
