using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using nhom5_webAPI.Models; // Bạn cần namespace này để dùng class User

namespace nhom5_webAPI.Services
{
    public class DynamicClaimTransformer : IClaimsTransformation
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DynamicClaimTransformer(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = principal.Identity as ClaimsIdentity;

            if (identity == null || !identity.IsAuthenticated || identity.HasClaim("transformed", "true"))
                return principal;

            var username = identity.Name;
            if (string.IsNullOrEmpty(username)) return principal;

            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return principal;

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                var roleEntity = await _roleManager.FindByNameAsync(role);
                if (roleEntity == null) continue;

                var roleClaims = await _roleManager.GetClaimsAsync(roleEntity);
                foreach (var claim in roleClaims)
                {
                    if (!identity.HasClaim(claim.Type, claim.Value))
                    {
                        identity.AddClaim(claim);
                    }
                }
            }

            identity.AddClaim(new Claim("transformed", "true")); // Đánh dấu đã load

            return principal;
        }
    }
}
