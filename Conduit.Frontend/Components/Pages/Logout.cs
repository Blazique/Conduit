using Microsoft.AspNetCore.Authorization;

namespace Conduit.Frontend.Components.Pages;

[Route("/Logout")]
[Authorize]
public class Logout
{
}
