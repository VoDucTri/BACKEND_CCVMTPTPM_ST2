using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace nhom5_webAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleClaimController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleClaimController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddClaimToRole([FromBody] RoleClaimModel model)
        {
            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null)
                return NotFound(new { Message = $"Role '{model.RoleName}' not found" });  // Thêm tên role vào thông báo lỗi

            // Kiểm tra nếu claim là 'all' (thêm quyền cho tất cả các loại)
            if (model.ClaimValue.EndsWith(".all"))
            {
                var prefix = model.ClaimValue.Split('.')[0]; 
                var claimsToAdd = new List<string>();

                if (prefix == "user")
                {
                    claimsToAdd.Add("user.view");
                }
                else if (prefix == "role")
                {
                    claimsToAdd.Add("role.view");
                    claimsToAdd.Add("role.create");
                    claimsToAdd.Add("role.delete");
                }
                else
                {
                    claimsToAdd.AddRange(new List<string>
            {
                $"{prefix}.view",
                $"{prefix}.create",
                $"{prefix}.edit",
                $"{prefix}.delete"
            });
                }

                foreach (var claimValue in claimsToAdd)
                {
                    var claim = new Claim("permission", claimValue);

                    // Kiểm tra xem claim đã tồn tại chưa, nếu chưa thì thêm vào
                    var existingClaim = (await _roleManager.GetClaimsAsync(role)).FirstOrDefault(c => c.Type == "permission" && c.Value == claimValue);
                    if (existingClaim == null)
                    {
                        var result = await _roleManager.AddClaimAsync(role, claim);
                        if (!result.Succeeded)
                            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
                    }
                }
            }
            else
            {
                // Thêm quyền khác (không phải quyền 'all')
                var claim = new Claim("permission", model.ClaimValue);
                var result = await _roleManager.AddClaimAsync(role, claim);

                if (!result.Succeeded)
                    return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
            }

            return Ok("Claim added to role");
        }


        // POST: api/RoleClaim/remove
        [HttpPost("remove")]
        public async Task<IActionResult> RemoveClaimFromRole([FromBody] RoleClaimModel model)
        {
            var role = await _roleManager.FindByNameAsync(model.RoleName);
            if (role == null) return NotFound("Role not found");

            if (model.ClaimValue.EndsWith(".all"))
            {
                // Xóa tất cả các quyền con của quyền 'all'
                var prefix = model.ClaimValue.Split('.')[0]; // Lấy phần trước dấu '.' (ví dụ: "product")
                var claimsToRemove = new List<string>
        {
            $"{prefix}.view",
            $"{prefix}.create",
            $"{prefix}.edit",
            $"{prefix}.delete"
        };

                foreach (var claimValue in claimsToRemove)
                {
                    var claim = new Claim("permission", claimValue);
                    var result = await _roleManager.RemoveClaimAsync(role, claim);

                    if (!result.Succeeded) return BadRequest(result.Errors);
                }
            }
            else
            {
                // Xóa quyền đơn lẻ
                var claim = new Claim("permission", model.ClaimValue);
                var result = await _roleManager.RemoveClaimAsync(role, claim);

                if (!result.Succeeded) return BadRequest(result.Errors);
            }

            return Ok("Claim removed from role");
        }


        // GET: api/RoleClaim/
        [HttpGet("{roleName}")]
        public async Task<IActionResult> GetClaimsOfRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound("Role not found");

            var claims = await _roleManager.GetClaimsAsync(role);
            var permissionClaims = claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value)
                .ToList();

            return Ok(permissionClaims);
        }
    }

    public class RoleClaimModel
    {
        public string RoleName { get; set; }
        public string ClaimValue { get; set; }
    }
}
