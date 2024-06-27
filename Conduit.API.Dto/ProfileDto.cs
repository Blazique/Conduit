namespace Conduit.API;

public record ProfileDto(string Id, string Username, string Bio, string Image, HashSet<string> FollowedBy);
