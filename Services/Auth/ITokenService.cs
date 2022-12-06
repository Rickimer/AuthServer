using AuthServer.AppConsts.Enums;

namespace AuthServer.Services.Auth
{
    public interface ITokenService
    {
        Task<(string token, DateTime lifeTime)> GetToken(string email, TokenType tokenType, JWTSettings _jwtSettings);
        Task<string> ValidateJwtToken(string token);
    }
}
