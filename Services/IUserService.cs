using AuthServer.DTO.User;

namespace AuthServer.Services
{
    public interface IUserService
    {
        Task CreateRole(string roleName);
        Task<UserDto> RegisterUser(UserAuthDto dto);
        Task<string> CheckUser(UserAuthDto dto);
    }
}
