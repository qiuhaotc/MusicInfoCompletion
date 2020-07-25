using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MusicInfoCompletion.Server
{
    public class SimpleAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        readonly AuthenticationInfo authenticationInfo;
        readonly IHostEnvironment configuration;

        public SimpleAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            AuthenticationInfo authenticationInfo,
            IHostEnvironment environment)
            : base(options, logger, encoder, clock)
        {
            this.authenticationInfo = authenticationInfo;
            this.configuration = environment;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (configuration.IsDevelopment())
            {
                var claimForDev = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Name, "Dev")
                };

                var identityForDev = new ClaimsIdentity(claimForDev, Scheme.Name);

                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identityForDev), Scheme.Name)));
            }

            if (authenticationInfo.Users == null || authenticationInfo.Users.Count == 0)
            {
                return Task.FromResult(AuthenticateResult.Fail("No Users Allowed Access"));
            }
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return Task.FromResult(AuthenticateResult.Fail("Missing Authorization Header"));
            }

            User allowedUser;

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(new[] { ':' }, 2);
                var username = credentials[0];
                var password = credentials[1];
                allowedUser = authenticationInfo.Users.FirstOrDefault(u => u.Password == password && u.UserName == username);
            }
            catch
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
            }

            if (allowedUser == null)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid Username or Password"));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, allowedUser.Id.ToString()),
                new Claim(ClaimTypes.Name, allowedUser.UserName)
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public class AuthenticationInfo
    {
        public IReadOnlyCollection<User> Users { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
