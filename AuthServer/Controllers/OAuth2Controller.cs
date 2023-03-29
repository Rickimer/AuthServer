using AuthServer.BLL.AppConst;
using AuthServer.BLL.Auth;
using AuthServer.BLL.DTO.User;
using AuthServer.BLL.Shared;
using AuthServer.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace AuthServer.Controllers
{
    /// <summary>
    /// Controller for handling user actions by using external oauth 2.0 authorization system
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class OAuth2Controller : Controller
    {
        private readonly IOAuth2Service _IOAuth2Service;
        private readonly ILogger<OAuth2Controller> _logger;
        private readonly ITokenService _tokenService;
        private readonly JWTSettings _jwtSettings;
        private readonly IUserService _userService;

        public OAuth2Controller(
            ILogger<OAuth2Controller> logger,
            IOAuth2Service IOAuth2Service,
            ITokenService tokenService,
            IOptions<JWTSettings> jwtSettings,
            IUserService userService
            )
        {
            _IOAuth2Service = IOAuth2Service;
            _logger = logger;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings.Value;
            _userService = userService;
        }

        [HttpPost("GitHubLogin")]
        public async Task<IActionResult> GitHubLogin([FromBody] GithubLoginDto githubLoginDto)
        {
            UserGithubRegisterDto gitHubUserData = await _IOAuth2Service.GitHubLogin(githubLoginDto.Code);
            if (gitHubUserData == null)
                return NotFound();

            _logger.LogInformation("LogIn " + gitHubUserData.Email);
            var tokenInfo = new TokenInfoDto
            {
                Email = gitHubUserData.Email,
                Login = gitHubUserData.Login,
                AuthSystemEnumId = AuthSystemEnumDto.Github,
                ConsumeServiceEnumId = ConsumeServiceEnumDto.Common
            };

            var user = await _userService.GetUserAsync(tokenInfo);
            if (user == null)
                user = await _userService.RegisterUser(gitHubUserData);

            var (accessToken, _) = await _tokenService.GetToken(tokenInfo, TokenType.Token, _jwtSettings, gitHubUserData);
            var (refreshToken, refreshTokenLifeTime) = await _tokenService.GetToken(tokenInfo, TokenType.RefreshToken, _jwtSettings, gitHubUserData);

            var token = SetToken(accessToken, refreshToken, refreshTokenLifeTime);

            return Ok(token);
        }

        private string SetToken(string accessToken, string refreshToken, DateTime _refreshTokenLifeTime)
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
