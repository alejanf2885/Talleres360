using System.Security.Claims;

namespace Talleres360.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int? GetTallerId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst("TallerId")?.Value;

            if (!int.TryParse(claim, out var tallerId))
                return null;

            return tallerId;
        }


        public static int GetUserId(this ClaimsPrincipal user)
        {
            var claim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(claim) ? 0 : int.Parse(claim);
        }

        public static string? GetClaimValue(this ClaimsPrincipal user, string claimType)
        {
            return user.FindFirst(claimType)?.Value;
        }
    }
}