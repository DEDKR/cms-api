using CmsApi.Repositories.Interfaces;
using System.Security.Claims;

namespace CmsApi.Security
{
    // Assignment policy: NEVER infer unrestricted rights from a client-provided RO.
    public sealed record MeetingAssignmentPermission(bool CanAssign, string? OfficeKeyCode,
        bool CanSeeAll, string Role)
    {
        public static async Task<MeetingAssignmentPermission> ResolveAsync(
            ClaimsPrincipal principal, IUserRepository users)
        {
            if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
                return new(false, null, false, "");
            var user = await users.GetByIdAsync(userId);
            if (user is null || !user.IsActive || user.IsPassChangeRequired ||
                (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.Now))
                return new(false, null, false, "");

            var role = user.Role?.Trim() ?? "";
            var isAdmin = user.RoleId == 1 || role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
            var isKurator = role.Equals("Kurator", StringComparison.OrdinalIgnoreCase);
            if (isAdmin) return new(true, null, true, "Admin");
            if (!isKurator) return new(false, null, false, role);
            var office = user.OfficeKeyCode?.Trim().ToUpperInvariant();
            if (office == "RO_00") return new(true, null, true, "Kurator");
            if (string.IsNullOrWhiteSpace(office)) return new(false, null, false, "Kurator");
            return new(true, office, false, "Kurator");
        }
    }
}
