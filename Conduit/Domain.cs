namespace Conduit.Domain;

public record User(int Id, string Email, string Token, string Username, string Bio, string Image);

internal record UserLoggedIn(User User);

public static class UserExtensions
{
    public static User ToUser(this ApiClient.User user) =>
        new(user.Id, user.Email, user.Token, user.Username, user.Bio, user.Image);
}

public delegate Task<Radix.Data.Result<User, string>>? Login(string? email, string? password);

public delegate Task<Radix.Data.Result<User, string[]>>? CreateUser(string? username, string? email, string? password);

public delegate Task<Radix.Data.Option<User>> GetUser();

