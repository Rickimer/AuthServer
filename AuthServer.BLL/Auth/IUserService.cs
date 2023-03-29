using AuthServer.BLL.AppConst;
using AuthServer.BLL.DTO.User;
using AuthServer.DAL.Data.Models;
using MessagesQueueService.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthServer.BLL.Auth
{
    public interface IUserService
    {
        Task CreateRole(string roleName);
        Task<string> RegisterUser(UserAuthDto dto);
        Task<User> RegisterUser(UserGithubRegisterDto dto);
        Task CheckUser(UserAuthDto dto);
        Task AddRefreshToken(string token, TokenInfoDto tokenInfo);
        Task CheckInSavedRefreshTokens(string token, TokenInfoDto tokenInfo);
        Task<User?> GetUserAsync(TokenInfoDto tokenInfo);
        Task<User?> FindByEmailAsync(string email, AuthSystemEnumDto authSystemId);
        Task<User?> FindByLoginAsync(string login, AuthSystemEnumDto authSystemId);
        Task<UserProfile> GetUserProfile(ClaimsPrincipal userPrincipal);
        Task UpdateUser(UserAuthDto userDto);
        Task<IEnumerable<UserGridBllDto>> GetUsers();
        Task ConfirmEmail(string item, string email);
        Task<SendActivationDto> NativeRegisterProcessing(UserAuthDto userDto);
    }
}
