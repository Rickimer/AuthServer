using AuthServer.BLL.AppConst;
using AuthServer.BLL.DTO.User;
using AuthServer.BLL.Shared;

namespace AuthServer.BLL.Auth
{
    public interface ITokenService
    {
        Task<(string token, DateTime lifeTime)> GetToken(TokenInfoDto tokenInfo, TokenType tokenType, JWTSettings _jwtSettings, UserBaseDto user);
        Task<(string token, DateTime lifeTime)> GetToken(TokenInfoDto tokenInfo, TokenType tokenType, JWTSettings _jwtSetting);
        Task<TokenInfoDto?> ValidateJwtToken(string token);
    }
}
