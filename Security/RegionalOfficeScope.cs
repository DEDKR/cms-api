using System.Security.Claims;
using CmsApi.Repositories.Interfaces;

namespace CmsApi.Security
{
    /// <summary>
    /// Server-only regional scope. Do not accept officeKeyCode from request payloads.
    /// A null OfficeKeyCode is valid ONLY when Allowed=true (Admin / RO_00).
    /// </summary>
    public sealed record RegionalOfficeScope(bool Allowed, string? OfficeKeyCode,
        int ErrorStatus = 403, string? ErrorMessage = null)
    {
        public static async Task<RegionalOfficeScope> ResolveAsync(
            ClaimsPrincipal principal, IUserRepository userRepository)
        {
            var claim = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(claim, out var userId))
                return new(false, null, 401, "User not authenticated");

            var user = await userRepository.GetByIdAsync(userId);
            if (user is null)
                return new(false, null, 401, "User not found");
            if (!user.IsActive)
                return new(false, null, 401, "Account is inactive");
            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.Now)
                return new(false, null, 401, "Account is temporarily locked");
            if (user.IsPassChangeRequired)
                return new(false, null, 403, "Password change is required");

            var office = user.OfficeKeyCode?.Trim().ToUpperInvariant();
            if (user.RoleId == 1 || office == "RO_00")
                return new(true, null);
            if (string.IsNullOrWhiteSpace(office))
                return new(false, null, 403, "No regional office assigned");

            return new(true, office);
        }
    }
}
