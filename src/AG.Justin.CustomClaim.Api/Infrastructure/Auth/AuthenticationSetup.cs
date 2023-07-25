using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace AG.Justin.CustomClaim.Api.Infrastructure.Auth;
public static class AuthenticationSetup
{
    public static IServiceCollection AddKeycloakAuth(this IServiceCollection services, CustomClaimConfiguration config)
    {
        IdentityModelEventSource.ShowPII = true;
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = config.Keycloak.RealmUrl;
            options.RequireHttpsMetadata = false;
            options.Audience = Clients.CustomClaimApi;
            options.MetadataAddress = config.Keycloak.WellKnownConfig;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidIssuers = new[] { config.Keycloak.RealmUrl, "http://localhost:8089/auth/realms/master" },
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("your-signing-key"))
            };
            options.Events = new JwtBearerEvents
            {
                OnTokenValidated = async context => await OnTokenValidatedAsync(context),
                OnAuthenticationFailed = async context => await OnAuthenticationFailedAsync(context),
                OnForbidden = context =>
                {
                    return Task.CompletedTask;
                },
                OnChallenge = async context => await OnChallengeAsync(context)
            };


        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.JustinUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var hasScope = context.User.HasClaim(c => c.Type.Equals(Claims.Scope, StringComparison.InvariantCulture)
                        && context.User.Claims.Any(c => c.Type.Equals(Claims.Scope, StringComparison.InvariantCulture) && c.Value.Contains(ScopeValueConstants.JustinUser)));
                    return hasScope;
                });
            });
        });
        return services;
    }
    private static Task OnChallengeAsync(JwtBearerChallengeContext context)
    {

        context.HandleResponse();
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        if (string.IsNullOrEmpty(context.Error))
            context.Error = "invalid_token";
        if (string.IsNullOrEmpty(context.ErrorDescription))
            context.ErrorDescription = "This request requires a valid JWT access token to be provided";

        return context.Response.WriteAsync(JsonConvert.SerializeObject(new
        {
            error = context.Error,
            error_description = context.ErrorDescription
        }));

    }

    private static Task OnAuthenticationFailedAsync(AuthenticationFailedContext context)
    {
        context.Response.OnStarting(async () =>
        {
            context.NoResult();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            string response =
            JsonConvert.SerializeObject("The access token provided is not valid.");
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
                response =
                    JsonConvert.SerializeObject("The access token provided has expired.");
            }
            await context.Response.WriteAsync(response);
        });

        //context.HandleResponse();
        //context.Response.WriteAsync(response).Wait();
        return Task.CompletedTask;
    }

    private static Task OnTokenValidatedAsync(TokenValidatedContext context)
    {
        if (context.Principal?.Identity is ClaimsIdentity identity
           && identity.IsAuthenticated)
        {
            // Flatten the Access claim here if needed and check required scope

            var hasScope = context.Principal.HasClaim(c => c.Type.Equals(Claims.Scope, StringComparison.InvariantCulture)
                        && c.Value.Contains(ScopeValueConstants.JustinUser));

            if (!hasScope)
            {
                context.Fail("Insufficient scope");
            }
            var token = context.SecurityToken as JwtSecurityToken;
            if (!token!.Audiences.Contains(Clients.CustomClaimApi))
            {
                context.Fail("Invalid audience");
            }

        }

        return Task.CompletedTask;
    }

}

