using AuthServer.BLL.AppConst;
using AuthServer.BLL.DTO.User;
using AuthServer.BLL.Shared;
using AuthServer.DAL.Data.Models;
//using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthServer.BLL.Auth
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<TokenService> _logger;
        private readonly JWTSettings _jwtSettings;
        private readonly DateTime _tokenMinutesLifeTime;
        private readonly DateTime _refreshTokenDaysLifeTime;
        private readonly IUserService _userService;

        public TokenService(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<JWTSettings> jwtSettings,
            ILogger<TokenService> logger,
            IUserService userService
            )
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
            _userManager = userManager;
            _tokenMinutesLifeTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(_jwtSettings.TokenMinutesLifeTime));
            _refreshTokenDaysLifeTime = DateTime.UtcNow.Add(TimeSpan.FromDays(_jwtSettings.RefreshTokenDaysLifeTime));            
            _userService = userService;
        }

        private async Task<List<Claim>> GetClaims(IdentityUser user, TokenInfoDto tokenInfo)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>();
            tokenInfo.ClaimsFromInfo(claims);
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())); //https://stackoverflow.com/questions/51119926/jwt-authentication-usermanager-getuserasync-returns-null

            if (tokenInfo.AuthSystemEnumId == AuthSystemEnumDto.NativeSystem)
            {
                foreach (var role in roles)
                {
                    claims.Add(new Claim("roles", role));
                }
            }
            else
                claims.Add(new Claim("roles", "ConfirmedEmail"));

            return claims;
        }
        
        //public async Task<(string token, DateTime lifeTime)> GetToken(TokenInfoDto tokenInfo, TokenType tokenType, JWTSettings _jwtSettings, User user)
        public async Task<(string token, DateTime lifeTime)> GetToken(TokenInfoDto tokenInfo, TokenType tokenType, JWTSettings _jwtSettings, UserBaseDto userBaseDto)
        {
            var user = (User)await _userManager.FindByEmailAsync(userBaseDto.Email);
            if (user == null)
            {
                throw new UserInterfaceException($"User with email {userBaseDto.Email} not found!");                
            }

            if (user.UserProfileId == null)
            {
                throw new UserInterfaceException($"UserProfile with email {userBaseDto.Email} not found!");
            }

            tokenInfo.UserId = user.UserProfileId.Value;
            if (tokenInfo.Login == null)
            {
                tokenInfo.Login = user.UserName;
            }

            var claims = await GetClaims(user, tokenInfo);
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            /*var secretKey = "MyKey_hasddsafklkn5ljk45nlknsdjkdfg5_kdfslk";
            var issuer = "RickToken";
            var audience = "AuthClient";
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));*/

            var lifeTime = tokenType == TokenType.Token ? _tokenMinutesLifeTime : _refreshTokenDaysLifeTime;

            var jwt = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    //issuer: issuer,
                    audience: _jwtSettings.Audience,
                    //audience: audience,
                    claims: claims,
                    expires: lifeTime,
                    notBefore: DateTime.UtcNow,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            if (tokenType == TokenType.RefreshToken)
            {
                await _userService.AddRefreshToken(token, tokenInfo);
            }

            return (token, lifeTime);
        }

        public async Task<(string token, DateTime lifeTime)> GetToken(TokenInfoDto tokenInfo, TokenType tokenType, JWTSettings _jwtSettings)
        {
            var userBaseDto = new UserBaseDto
            {
                Email = tokenInfo.Email,
                Login = tokenInfo.Login,
            };

            return await GetToken(tokenInfo, tokenType, _jwtSettings, userBaseDto);
        }
        /*public async Task<(string token, DateTime lifeTime)> GetToken(TokenInfoDto tokenInfo, TokenType tokenType, JWTSettings _jwtSettings)
        {
            var user = await _userService.GetUserAsync(tokenInfo);
            if (user == null)
                throw new UserInterfaceException("user not found");

            return await GetToken(tokenInfo, tokenType, _jwtSettings, user);
        }*/

        public async Task<TokenInfoDto?> ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey = key,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            if (DateTime.Compare(validatedToken.ValidTo, DateTime.UtcNow) < 0)
            {
                return null;
            }

            var jwtToken = (JwtSecurityToken)validatedToken;

            var tokenInfo = new TokenInfoDto();
            tokenInfo.SetFromClaims(jwtToken.Claims);

            await _userService.CheckInSavedRefreshTokens(token, tokenInfo);

            return tokenInfo;
        }

    }
}
