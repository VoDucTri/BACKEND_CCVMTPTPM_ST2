using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using nhom5_webAPI.Models;
using nhom5_webAPI.Models.nhom5_webAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace nhom5_webAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthenticateController(
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userExists = await _userManager.FindByNameAsync(model.Username);
            if (userExists != null)
                return BadRequest(new { Status = false, Message = "User already exists" });

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email,
                Address = model.Address,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = false, Message = "User creation failed" });

            if (!await _roleManager.RoleExistsAsync("User"))
                await _roleManager.CreateAsync(new IdentityRole("User"));

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { Status = true, Message = "User registered successfully with 'User' role" });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByNameAsync(model.Username);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new { Status = false, Message = "Invalid username or password" });

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = GenerateToken(authClaims);

            return Ok(new
            {
                Status = true,
                Token = token,
                User = new
                {
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = userRoles
                }
            });
        }

        [Authorize(Policy = "User.View")]
        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = _userManager.Users.ToList();
                var userList = new List<object>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userList.Add(new
                    {
                        Username = user.UserName,
                        Email = user.Email,
                        FullName = user.FullName,
                        Address = user.Address,
                        PhoneNumber = user.PhoneNumber,
                        Roles = roles
                    });
                }

                return Ok(new
                {
                    Status = true,
                    Message = "User list retrieved successfully",
                    Users = userList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = false,
                    Message = "An error occurred while retrieving the user list",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest(new { Status = false, Message = "Username is required" });

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return NotFound(new { Status = false, Message = "User not found. Please register before using this feature." });

            var otpCode = new Random().Next(100000, 999999).ToString();
            user.PasswordResetCode = otpCode ?? string.Empty;
            user.ResetCodeExpiration = DateTime.UtcNow.AddMinutes(15);

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return StatusCode(500, new { Status = false, Message = "Failed to generate OTP. Please try again." });

            return Ok(new { Status = true, otp = otpCode, Message = "OTP generated successfully" });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null || user.PasswordResetCode != request.ResetCode || user.ResetCodeExpiration < DateTime.UtcNow)
                return BadRequest("Invalid or expired reset code");

            var result = await _userManager.RemovePasswordAsync(user);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error removing old password");

            result = await _userManager.AddPasswordAsync(user, request.NewPassword);
            if (!result.Succeeded)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error setting new password");

            user.PasswordResetCode = string.Empty;
            user.ResetCodeExpiration = null;
            await _userManager.UpdateAsync(user);

            return Ok("Password has been reset");
        }

        private string GenerateToken(IEnumerable<Claim> claims)
        {
            var jwtSettings = _configuration.GetSection("JWTKey");
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = jwtSettings["ValidIssuer"],
                Audience = jwtSettings["ValidAudience"],
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
