using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using nhom5_webAPI.Models;

namespace nhom5_webAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class UserRoleController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoleController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddUserToRole([FromBody] UserRoleModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return NotFound("User not found");

            var roleExists = await _roleManager.RoleExistsAsync(model.RoleName);
            if (!roleExists) return NotFound("Role does not exist");

            var result = await _userManager.AddToRoleAsync(user, model.RoleName);
            if (result.Succeeded) return Ok("User added to role");

            return BadRequest(result.Errors);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveUserFromRole([FromBody] UserRoleModel model)
        {
            var user = await _userManager.FindByNameAsync(model.UserName);
            if (user == null) return NotFound("User not found");

            var result = await _userManager.RemoveFromRoleAsync(user, model.RoleName);
            if (result.Succeeded) return Ok("User removed from role");

            return BadRequest(result.Errors);
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetRolesOfUser(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return NotFound("User not found");

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(roles);
        }
    }

    public class UserRoleModel
    {
        public string UserName { get; set; }
        public string RoleName { get; set; }
    }
}
