using JobApplica.DataContext;
using JobApplica.LoginRegister;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using System.Security.Claims;

namespace JobApplica.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;

        public AccountController(ITokenService tokenService, UserManager<AppUser> userManager, ApplicationDbContext dbContext, SignInManager<AppUser> signInManager)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _dbContext = dbContext;
            _signInManager = signInManager;
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            return Ok(new
            {
                Email = User.FindFirstValue(ClaimTypes.Email),
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                FirstName = User.FindFirstValue(ClaimTypes.GivenName)
            });
        }

        private async Task<bool>UserExists(string email)
        {
            return await _userManager.FindByEmailAsync(email) is not null;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>>Register(RegisterDto registerDto)
        {
            if (await UserExists(registerDto.Email))
            {
                return BadRequest("Email Already Taken");
            }
                

            var user = new AppUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                

            };

            var allowedRoles = new[] { "JobSeeker", "Employer" };

            if (!allowedRoles.Contains(registerDto.Role))
            {
                return BadRequest("Invalid role");
            }


            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            var roleResult = await _userManager.AddToRoleAsync(user, registerDto.Role);

            
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            var roles = await _userManager.GetRolesAsync(user);


            return new UserDto
            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? string.Empty,
                Token = await _tokenService.CreateTokenAsync(user)
            };
        }

        [HttpPost("Login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return Unauthorized("Invalid Email or Password");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user,loginDto.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized("Invalid Email Or Password");
            }

            var roles = await _userManager.GetRolesAsync(user);
            return new UserDto

            {
                Email = user.Email,
                FirstName = user.FirstName,
                LastName= user.LastName,
                Role = roles.FirstOrDefault() ?? string.Empty,
                Token = await _tokenService.CreateTokenAsync(user)
            };
        }


    }
}
