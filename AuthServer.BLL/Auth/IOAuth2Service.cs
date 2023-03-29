using AuthServer.BLL.DTO.User;

namespace AuthServer.BLL.Auth
{
    public interface IOAuth2Service
    {
        Task<UserGithubRegisterDto> GitHubLogin(string code);
    }
}
