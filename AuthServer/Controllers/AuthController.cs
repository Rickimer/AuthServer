using AuthServer.BLL.AppConst;
using AuthServer.BLL.Auth;
using AuthServer.BLL.DTO.User;
using AuthServer.BLL.Shared;
using AuthServer.Shared;
using AutoMapper;
using MessagesQueueService.RabbitMQ;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthServer.Controllers
{
    /// <summary>
    /// Controller for handling user actions in a native system
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JWTSettings _jwtSettings;
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;        
        private readonly ILogger<AuthController> _logger;
        private readonly IMapper _mapper;        

        public AuthController(UserManager<IdentityUser> userManager,
            IOptions<JWTSettings> jwtSettings,
            ILogger<AuthController> logger,
            ITokenService tokenService,            
            IUserService userService,
            IMapper mapper
            )
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
            _tokenService = tokenService;            
            _userService = userService;
            _mapper = mapper;            
        }

        [HttpGet("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var clientRefreshToken = Request.Cookies["jwt"];
            if (clientRefreshToken == null)
            {
                return Unauthorized("Error validating access token: Session has expired");
            }

            var tokenInfo = await _tokenService.ValidateJwtToken(clientRefreshToken);
            if (tokenInfo == null)
            {
                return BadRequest("Bad RefreshToken");
            }

            var (accessToken, _) = await _tokenService.GetToken(tokenInfo, TokenType.Token, _jwtSettings);
            var (refreshToken, refreshTokenLifeTime) = await _tokenService.GetToken(tokenInfo, TokenType.RefreshToken, _jwtSettings);

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
            _logger.LogWarning($"user {userDto.Login} with email {userDto.Email} login");

            if (userDto == null || (String.IsNullOrEmpty(userDto.Password)))
            {
                return BadRequest("Empty user password");
            }

            if (userDto.Email == null)
            {
                return BadRequest("Empty user email");
            }

            string token = String.Empty;
            try
            {
                
            var tokenInfo = new TokenInfoDto
            {
                Email = userDto.Email,
                Login = userDto.Login,
                AuthSystemEnumId = AuthSystemEnumDto.NativeSystem,
                ConsumeServiceEnumId = ConsumeServiceEnumDto.Common                
            };
            _logger.LogInformation($"LogIn user {tokenInfo.Login}, email {userDto.Email}");

                var (accessToken, _) = await _tokenService.GetToken(tokenInfo, TokenType.Token, _jwtSettings, userDto);
                var (refreshToken, refreshTokenLifeTime) = await _tokenService.GetToken(tokenInfo, TokenType.RefreshToken, _jwtSettings, userDto);

                token = setToken(accessToken, refreshToken, refreshTokenLifeTime);
            }
            catch (UserInterfaceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(token);
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
                var userAuthDto = _mapper.Map<UserAuthDto>(userDto);                
                var sendActivationDto = await _userService.NativeRegisterProcessing(userAuthDto);

                var confirmUrl = Url.Action("ConfirmEmail", "User", new { item = sendActivationDto.ConfirmUrl, email = userDto.Email }, Request.Scheme);
                sendActivationDto.ConfirmUrl = confirmUrl;
                var s = new RabbitMQService();
                await s.SendActivationEmailAsync(sendActivationDto);
            }
            catch (UserInterfaceException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
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
