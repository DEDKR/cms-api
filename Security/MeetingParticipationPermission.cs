using CmsApi.Repositories.Interfaces;
using System.Security.Claims;

namespace CmsApi.Security
{
    public sealed record MeetingParticipationPermission(
        bool Allowed, long UserId, bool CanManageOthers, bool CanSeeAll,
        string? OfficeKeyCode, string Role)
    {
        public static async Task<MeetingParticipationPermission> ResolveAsync(
            ClaimsPrincipal principal, IUserRepository users)
        {
            if (!int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
                return new(false, 0, false, false, null, "");

            var user = await users.GetByIdAsync(id);
            if (user is null || !user.IsActive || user.IsPassChangeRequired ||
                (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.Now))
                return new(false, 0, false, false, null, "");

            var role = user.Role?.Trim() ?? "";
            var office = user.OfficeKeyCode?.Trim().ToUpperInvariant();
            var global = user.RoleId == 1 || role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                         || office == "RO_00";
            if (global)
                return new(true, id, true, true, null, role);

            if (string.IsNullOrWhiteSpace(office))
                return new(false, id, false, false, null, role);

            if (role.Equals("Kurator", StringComparison.OrdinalIgnoreCase))
                return new(true, id, true, false, office, role);

            if (role.Equals("İstifadəçi", StringComparison.OrdinalIgnoreCase))
                return new(true, id, false, false, office, role);

            return new(false, id, false, false, office, role);
        }
    }
}
