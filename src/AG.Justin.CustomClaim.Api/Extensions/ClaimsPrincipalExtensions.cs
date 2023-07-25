﻿using AG.Justin.CustomClaim.Api.Infrastructure.Auth;
using NodaTime;
using NodaTime.Text;
using System.Security.Claims;
using System.Text.Json;

namespace AG.Justin.CustomClaim.Api.Extensions;
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Returns the UserId of the logged in user (from the 'sub' claim). If there is no logged in user, this will return Guid.Empty
    /// </summary>
    public static Guid GetUserId(this ClaimsPrincipal? user)
    {
        var userId = user?.FindFirstValue(Claims.Subject);

        return Guid.TryParse(userId, out var parsed)
            ? parsed
            : Guid.Empty;
    }

    /// <summary>
    /// Returns the Birthdate Claim of the User, parsed in ISO format (yyyy-MM-dd)
    /// </summary>
    public static LocalDate? GetBirthdate(this ClaimsPrincipal user)
    {
        var birthdate = user.FindFirstValue(Claims.Birthdate);

        var parsed = LocalDatePattern.Iso.Parse(birthdate!);
        if (parsed.Success)
        {
            return parsed.Value;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Returns the Gender Claim of the User, parsed in ISO format (M/F)
    /// </summary>
    public static string? GetGender(this ClaimsPrincipal user)
    {
        var gender = user.FindFirstValue(Claims.Gender);

        if (string.IsNullOrEmpty(gender))
            return null;

        return gender;
    }

    /// <summary>
    /// Returns the Identity Provider of the User, or null if User is null
    /// </summary>
    public static string? GetIdentityProvider(this ClaimsPrincipal? user) => user?.FindFirstValue(Claims.IdentityProvider);

    /// <summary>
    /// check wheather the user is a valid bcps user using ad groups
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public static IEnumerable<string> GetUserRoles(this ClaimsIdentity identity)
    {
        var roleClaim = identity.Claims
           .SingleOrDefault(claim => claim.Type == Claims.ResourceAccess)
           ?.Value;

        if (string.IsNullOrWhiteSpace(roleClaim))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            var userRoles = JsonSerializer.Deserialize<Dictionary<string, ResourceAccess>>(roleClaim, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return userRoles?.TryGetValue(roleClaim, out var access) == true
                ? access.Roles
                : Enumerable.Empty<string>();
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }
    /// <summary>
    /// Parses the Resource Access claim and returns the roles for the given resource
    /// </summary>
    /// <param name="resourceName">The name of the resource to retrive the roles from</param>
    public static IEnumerable<string> GetResourceAccessRoles(this ClaimsIdentity identity, string resourceName)
    {
        var resourceAccessClaim = identity.Claims
            .SingleOrDefault(claim => claim.Type == Claims.ResourceAccess)
            ?.Value;

        if (string.IsNullOrWhiteSpace(resourceAccessClaim))
        {
            return Enumerable.Empty<string>();
        }

        try
        {
            var resources = JsonSerializer.Deserialize<Dictionary<string, ResourceAccess>>(resourceAccessClaim, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            return resources?.TryGetValue(resourceName, out var access) == true
                ? access.Roles
                : Enumerable.Empty<string>();
        }
        catch
        {
            return Enumerable.Empty<string>();
        }
    }

    private class ResourceAccess
    {
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }
}

