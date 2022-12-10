using AuthServer.AppConsts.Enums;
using AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthServer.Services.Auth
{
    public class TokenService : ITokenService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<TokenService> _logger;
        private readonly JWTSettings _jwtSettings;
        private readonly DateTime _tokenMinutesLifeTime;
        private readonly DateTime _refreshTokenDaysLifeTime;

        public TokenService(UserManager<IdentityUser> userManager,
            IOptions<JWTSettings> jwtSettings,
            ILogger<TokenService> logger
            )
        {
            _jwtSettings = jwtSettings.Value;
            _logger = logger;
            _userManager = userManager;
            _tokenMinutesLifeTime = DateTime.UtcNow.Add(TimeSpan.FromMinutes(_jwtSettings.TokenMinutesLifeTime));
            _refreshTokenDaysLifeTime = DateTime.UtcNow.Add(TimeSpan.FromDays(_jwtSettings.RefreshTokenDaysLifeTime));
        }

        private async Task<List<Claim>> GetClaims(IdentityUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>();
            claims.Add(new Claim("username", user.UserName));
            claims.Add(new Claim("email", user.Email));

            if (user.EmailConfirmed)
            {
                claims.Add(new Claim("roles", "ConfirmedEmail"));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim("roles", role));
            }

            return claims;
        }

        public async Task<(string token, DateTime lifeTime)> GetToken(string email, TokenType tokenType, JWTSettings _jwtSettings)
        {
            _logger.LogTrace("GetToken");
            var user = (User)await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("user not found");

            var claims = await GetClaims(user);
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

            var lifeTime = tokenType == TokenType.Token ? _tokenMinutesLifeTime : _refreshTokenDaysLifeTime;

            var jwt = new JwtSecurityToken(
                    issuer: _jwtSettings.Issuer,
                    audience: _jwtSettings.Audience,
                    claims: claims,
                    expires: lifeTime,
                    notBefore: DateTime.UtcNow,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

            var token = new JwtSecurityTokenHandler().WriteToken(jwt);
            if (tokenType == TokenType.RefreshToken)
            {
                user.RefreshToken = token;
                user.LastCreatedRefreshToken = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);
            }

            return (token, lifeTime);
        }

        public async Task<string> ValidateJwtToken(string token)
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

            var jwtToken = (JwtSecurityToken)validatedToken;
            var email = jwtToken.Claims.First(x => x.Type == "email").Value;

            var user = (User)await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("user not found");

            if (!String.Equals(user.RefreshToken, token, StringComparison.InvariantCulture))
            {
                throw new Exception("wrong refreshToken");
            }

            return email;
        }
    }
}
