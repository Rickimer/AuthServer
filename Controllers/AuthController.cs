using AuthServer.AppConsts.Enums;
using AuthServer.DTO.User;
using AuthServer.Models;
using AuthServer.Models.Repository;
using AuthServer.Services;
using AuthServer.Services.Auth;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JWTSettings _jwtSettings;
        private readonly UserService _userService;
        private readonly ITokenService _tokenService;
        private readonly IMailService _mailService;
        private readonly ILogger<AuthController> _logger;        

        public AuthController(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JWTSettings> jwtSettings,            
            ILogger<AuthController> logger,
            IMapper mapper,
            ITokenService tokenService,
            IMailService mailService            
            )
        { 
            _userManager = userManager;            
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
            _tokenService = tokenService;
            _mailService = mailService;
            _userService = new UserService(_userManager, roleManager, mapper);            
        }

        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var clientRefreshToken = Request.Cookies["jwt"];
            if (clientRefreshToken == null)
            {
                return Unauthorized("Error validating access token: Session has expired");                
            }

            var email = await _tokenService.ValidateJwtToken(clientRefreshToken);
            var (accessToken, _) = await _tokenService.GetToken(email, TokenType.Token, _jwtSettings); //передать jwtsettings в констуктор
            var (refreshToken, refreshTokenLifeTime) = await _tokenService.GetToken(email, TokenType.RefreshToken, _jwtSettings);

            var token = JsonConvert.SerializeObject(new { accessToken });

            Response.Cookies.Delete("jwt");

            var cookieOptions = new CookieOptions()
            {
                IsEssential = true,                
                Expires = refreshTokenLifeTime,
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("jwt", refreshToken, cookieOptions);

            return Ok(token);            
        }

        [HttpPost("LogIn")]
        public async Task<IActionResult> LogIn(UserLoginDto userDto)
        {
            if (userDto == null || (String.IsNullOrEmpty(userDto.Password)))
            {
                return BadRequest("Empty user password");
            }

            var user = await _userManager.FindByEmailAsync(userDto.Email);
            if (user == null)
            {
                return BadRequest($"User with email {userDto.Email} not found!");
            }

            _logger.LogInformation("LogIn " + userDto.Email);
            var (accessToken, _) = await _tokenService.GetToken(userDto.Email, TokenType.Token, _jwtSettings);
            var (refreshToken, refreshTokenLifeTime) = await _tokenService.GetToken(userDto.Email, TokenType.RefreshToken, _jwtSettings);

            var token = setToken(accessToken, refreshToken, refreshTokenLifeTime);
            return Ok(token);
        }

        [HttpPost("LogOut")]        
        public IActionResult LogOut()
        {
            Response.Cookies.Delete("jwt");
            return Ok();
        }

        [HttpGet("Relogin")]
        public IActionResult ReLogin(string ReturnUrl)
        {
            return StatusCode(403);            
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserPostDto userDto)
        {
            if (userDto == null || (String.IsNullOrEmpty(userDto.Password)))
            {
                return BadRequest("Empty user password");
            }

            try
            {
                var user = await _userManager.FindByEmailAsync(userDto.Email);
                if (user != null)
                {
                    return Conflict($"User with email {userDto.Email} is already exist");
                }
                if (!String.IsNullOrEmpty(userDto.UserName))
                {
                    user = await _userManager.FindByNameAsync(userDto.UserName);
                    if (user != null)
                    {
                        return Conflict($"User {userDto.UserName} is already exist");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            var error = await _userService.CheckUser(userDto);
            if (!String.IsNullOrEmpty(error))
            {
                return Conflict(error);
            }
            var result = await _userService.RegisterUser(userDto);
            if (result == null)
                return BadRequest();

            var activationLink = Url.Action("ConfirmEmail", "User", new { item=result.PasswordHash, email = userDto.Email }, Request.Scheme);
            if (activationLink == null)
                return BadRequest();
            
            await _mailService.SendActivationMail(userDto.Email, "subject", activationLink); 

            return Ok();
        }

        private string setToken(string accessToken, string refreshToken, DateTime _refreshTokenLifeTime)
        {
            var cookieOptions = new CookieOptions()
            {
                IsEssential = true,
                Expires = _refreshTokenLifeTime,
                Secure = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None
            };

            Response.Cookies.Append("jwt", refreshToken, cookieOptions);
            return JsonConvert.SerializeObject(new { accessToken });
        }
    }
}
