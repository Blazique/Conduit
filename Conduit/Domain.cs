using Conduit.ApiClient;

namespace Conduit.Domain;

public record User(int Id, string Email, string Token, string Username, string Bio, string Image);

public record Profile(string Username, string Bio, string Image, bool Following);

internal record UserLoggedIn(User User);

public static class DtoExtensions
{
    public static User ToUser(this ApiClient.User user) =>
        new(user.Id, user.Email, user.Token, user.Username, user.Bio, user.Image);

    public static Profile ToProfile(this ProfileDto profile) => 
        new(profile.Username, profile.Bio, profile.Image, profile.Following);
}

public delegate Task<Radix.Data.Result<User, string>>? Login(string? email, string? password);

public delegate Task<Radix.Data.Result<User, string[]>>? CreateUser(string? username, string? email, string? password);

public delegate Task<Radix.Data.Option<User>> GetUser();

public delegate Task<Radix.Data.Option<ProfileDto>> GetProfile(string username);


