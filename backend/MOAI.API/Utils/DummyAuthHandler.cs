using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class DummyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DummyAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var request = Context.Request;

        var email = Uri.UnescapeDataString(request.Cookies["user_email"] ?? "");
        var role = request.Cookies["user_role"] ?? "";
        var department = request.Cookies["user_department"] ?? "";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role))
        {
            return Task.FromResult(AuthenticateResult.Fail("Missing user cookies"));
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("Department", department)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
