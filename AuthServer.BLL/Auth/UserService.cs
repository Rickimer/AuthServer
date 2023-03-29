using AuthServer.BLL.AppConst;
using AuthServer.BLL.DTO.User;
using AuthServer.BLL.Shared;
using AuthServer.DAL.Data.Enums;
using AuthServer.DAL.Data.Models;
using AuthServer.DAL.Data.Repository;
using AutoMapper;
using MessagesQueueService.RabbitMQ;
using MessagesQueueService.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AuthServer.BLL.Auth
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IRepository<UserServiceProfile> _userServiceProfileRepository;
        private readonly IRepository<UserProfile> _userProfileRepository;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly IMQServices _MQServices;

        public UserService(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IRepository<UserProfile> userProfileRepository,
            IRepository<UserServiceProfile> userServiceProfileRepository,
            IMapper mapper,
            IRepository<RefreshToken> refreshTokenRepository,
            IMQServices MQServices
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userServiceProfileRepository = userServiceProfileRepository;
            _userProfileRepository = userProfileRepository;
            _mapper = mapper;
            _refreshTokenRepository = refreshTokenRepository;
            _MQServices = MQServices;            
        }

        public async Task CreateRole(string roleName)
        {
            if (!(await _roleManager.RoleExistsAsync(roleName)))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        public async Task CheckUser(UserAuthDto dto)
        {
            if (dto.Email != null)
            {
                var user = await _userManager.FindByEmailAsync(dto.Email);
                if (user != null)
                {
                    throw new UserInterfaceException($"Email is {dto.Email} already registered");
                }
            }

            if (dto.Login != null)
            {
                var userName = await _userManager.FindByNameAsync(dto.Login);
                if (userName != null)
                {
                    throw new UserInterfaceException($"username {userName} is already registered");
                }
            }
        }

        private async Task AddProfile(User user, UserProfileDto userProfileDto, AuthSystemEnumDto authSystemEnumDto, ConsumeServiceEnumDto consumeServiceEnumDto)
        {
            var userServiceProfileDto = new UserServiceProfileDto
            {
                ServiceId = consumeServiceEnumDto
            };

            var userServiceProfile = _mapper.Map<UserServiceProfile>(userServiceProfileDto);

            userProfileDto.AuthSystemId = authSystemEnumDto;            
            userProfileDto.UserServiceProfilesDto.Add(userServiceProfileDto);

            var userProfile = _mapper.Map<UserProfile>(userProfileDto);
            userProfile.UserServiceProfiles.Add(userServiceProfile);
            userProfile.User = user;
            await _userProfileRepository.AddAsync(userProfile);

            user.UserProfile = userProfile;
            var identityUser = (IdentityUser)user;
            await _userManager.UpdateAsync(identityUser);
            //await _userManager.UpdateAsync(user);
        }

        public async Task<User> RegisterUser(UserGithubRegisterDto dto)
        {
            var userProfile = _mapper.Map<UserProfileDto>(dto);
            var user = new User
            {
                UserName = dto.Login,
            };
            await _userManager.CreateAsync(user);
            await AddProfile(user, userProfile, AuthSystemEnumDto.Github, ConsumeServiceEnumDto.Common);

            return user;
        }

        public async Task<string> RegisterUser(UserAuthDto dto)
        {
            if (dto.Password == null)
            {
                throw new ArgumentException("Password");
            }
            if (dto.Login == null)
            {
                throw new ArgumentException("Login");
            }

            var user = new User { UserName = dto.Login, Email = dto.Email, EmailConfirmed = false };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                throw new Exception("Error creating user" + result.Errors);
            }

            if (dto.Roles != null) //Roles - user created from adminpanel. Now only for NativeSystem
            {
                if (dto.Roles.Contains("ConfirmedEmail"))
                {
                    user.EmailConfirmed = true;
                }

                var roles = await _userManager.GetRolesAsync(user);

                var newRoles = dto.Roles.Except(roles);
                var rolesToDelete = roles.Except(dto.Roles);

                foreach (var role in newRoles)
                {
                    await CreateRole(role);
                }
                await _userManager.AddToRolesAsync(user, newRoles);
                await _userManager.RemoveFromRolesAsync(user, rolesToDelete);
            }
            
            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, dto.Password);            

            var userProfile = new UserProfileDto
            {
                Email = dto.Email,
                Login = dto.Login
            };
            await AddProfile(user, userProfile, AuthSystemEnumDto.NativeSystem, ConsumeServiceEnumDto.Common);
            
            return user.PasswordHash;
        }

        public async Task AddRefreshToken(string token, TokenInfoDto tokenInfo)
        {
            UserServiceProfile? serviceProfile = null;

            if (tokenInfo.ServiceProfileId == null)
            {
                var user = await GetUserAsync(tokenInfo);
                if (user == null)
                    throw new UserInterfaceException("user not found");

                var consumeServiceEnum = _mapper.Map<ConsumeServiceEnum>(tokenInfo.ConsumeServiceEnumId);
                serviceProfile = (await _userServiceProfileRepository.GetAsync(w => w.UserProfileId == user.UserProfileId
                    && w.ServiceId == consumeServiceEnum)).FirstOrDefault();
            }
            else
                serviceProfile = await _userServiceProfileRepository.GetByIDAsync(tokenInfo.ServiceProfileId.Value);

            if (serviceProfile == null)
                throw new UserInterfaceException($"User is not registered for the service {tokenInfo.ConsumeServiceEnumId}");

            var oldTokens = serviceProfile.RefreshTokens;
            if (oldTokens != null)
            {
                foreach (var oldToken in oldTokens)
                {
                    await _refreshTokenRepository.Delete(oldToken);
                }
            }

            var refreshToken = new RefreshToken
            {
                Token = token,
            };
            serviceProfile.RefreshTokens?.Add(refreshToken);
            await _userServiceProfileRepository.UpdateAsync(serviceProfile);
        }

        public async Task CheckInSavedRefreshTokens(string token, TokenInfoDto tokenInfo)
        {
            var user = await GetUserAsync(tokenInfo);

            if (user == null)
                throw new UserInterfaceException("user not found");

            var consumeServiceEnum = _mapper.Map<ConsumeServiceEnum>(tokenInfo.ConsumeServiceEnumId);
            var serviceProfile = (await _userServiceProfileRepository.GetAsync(w => w.UserProfileId == user.UserProfileId
                && w.ServiceId == consumeServiceEnum, null, "RefreshTokens"))
                .FirstOrDefault();

            if (serviceProfile == null) throw new UserInterfaceException($"user is not registered for the service {tokenInfo.ConsumeServiceEnumId}");

            var savedToken = serviceProfile.RefreshTokens?.FirstOrDefault(e => e.Token.Equals(token, StringComparison.InvariantCulture));
            if (savedToken == null) throw new Exception("wrong refreshToken");

            tokenInfo.ServiceProfileId = serviceProfile.Id;
        }

        public async Task<User?> GetUserAsync(TokenInfoDto tokenInfo)
        {
            User? user = null;
            if (tokenInfo == null) throw new ArgumentNullException(nameof(tokenInfo));
            
            if (tokenInfo.Email != null)
            {
                user = await FindByEmailAsync(tokenInfo.Email, tokenInfo.AuthSystemEnumId);

                if (user != null)
                    return user;
            }

            if (tokenInfo.Login != null)
            {
                user = await FindByLoginAsync(tokenInfo.Login, tokenInfo.AuthSystemEnumId);
            }

            return user;
        }

        public async Task<User?> FindByEmailAsync(string email, AuthSystemEnumDto authSystemEnumDto)
        {
            if (email == null) throw new ArgumentNullException("email");
            if (authSystemEnumDto == AuthSystemEnumDto.NativeSystem)
                return (User)await _userManager.FindByEmailAsync(email);

            var authSystemId = _mapper.Map<AuthSystemEnum>(authSystemEnumDto);
            var profile = await _userProfileRepository.GetAsync(w => w.AuthSystemId == authSystemId && w.Email == email, null, "User");

            if (profile.Count() > 1)
                throw new Exception($"non-unique email value {email} for authSystem {authSystemId}");

            return profile.Select(e => e.User).FirstOrDefault();
        }

        public async Task<User?> FindByLoginAsync(string login, AuthSystemEnumDto authSystemEnumDto)
        {
            if (login == null) throw new ArgumentNullException("login");
            if (authSystemEnumDto == AuthSystemEnumDto.NativeSystem)
                return (User)await _userManager.FindByNameAsync(login);

            var authSystemId = _mapper.Map<AuthSystemEnum>(authSystemEnumDto);
            var profile = await _userProfileRepository.GetAsync(w => w.AuthSystemId == authSystemId && w.Login == login, null, "User");

            if (profile.Count() > 1)
                throw new Exception($"non-unique login value {login} for authSystem {authSystemId}");

            return profile.Select(e => e.User).FirstOrDefault();
        }

        public async Task<UserProfile> GetUserProfile(ClaimsPrincipal userPrincipal)
        {
            var user = (User)await _userManager.GetUserAsync(userPrincipal);
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (user.UserProfileId == null)
                throw new ArgumentNullException("UserProfileId");

            var profile = await _userProfileRepository.GetByIDAsync(user.UserProfileId.Value);

            if (profile == null)
                throw new ArgumentNullException("profile");

            return profile;
        }

        public async Task UpdateUser(UserAuthDto userDto)
        {
            if (userDto == null)
                throw new UserInterfaceException("No user to update");

            if (userDto.Email == null)
                throw new UserInterfaceException("No user email to update");

            var user = (User?)await _userManager.FindByEmailAsync(userDto.Email);
            if (user == null)
            {
                throw new UserInterfaceException("User not finded!");
            }

            _mapper.Map(userDto, user);

            if (userDto.Roles != null)
            {
                if (userDto.Roles.Contains("ConfirmedEmail"))
                {
                    userDto.Roles.Remove("ConfirmedEmail");
                    user.EmailConfirmed = true;
                }
                else
                {
                    user.EmailConfirmed = false;
                }

                var roles = await _userManager.GetRolesAsync(user);

                var newRoles = userDto.Roles.Except(roles);
                var rolesToDelete = roles.Except(userDto.Roles);

                foreach (var role in newRoles)
                {
                    await CreateRole(role);
                }
                await _userManager.AddToRolesAsync(user, newRoles);
                await _userManager.RemoveFromRolesAsync(user, rolesToDelete);
            }

            if (!String.IsNullOrEmpty(userDto.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userDto.Password);
            }
            await _userManager.UpdateAsync(user);
        }

        public async Task<IEnumerable<UserGridBllDto>> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersDto = new List<UserGridBllDto>();

            int i = 0;
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (user.EmailConfirmed)
                {
                    roles.Add("ConfirmedEmail");
                }
                var userDto = new UserGridBllDto
                {
                    Login = user.UserName,
                    Roles = roles,
                    Email = user.Email,
                    Id = i
                };

                usersDto.Add(userDto);
                i++;
            }

            return usersDto;
        }

        public async Task<SendActivationDto> NativeRegisterProcessing(UserAuthDto userDto)
        {            
                await CheckUser(userDto);
                var passwordHash = await RegisterUser(userDto);
                                
                var sendActivationDto = new SendActivationDto { 
                    ToEmail = userDto.Email,
                    ConfirmUrl = passwordHash
                };

            return sendActivationDto;                
        }

        public async Task ConfirmEmail(string item, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found!");

            if (user.EmailConfirmed)
                throw new UserInterfaceException($"User email {email} already confirmed!");

            var hash = item;

            if (user.PasswordHash != hash)
            {
                throw new UserInterfaceException($"Not a valid user identification!");
            }

            user.EmailConfirmed = true;
            await CreateRole("ConfirmedEmail");
            await _userManager.AddToRoleAsync(user, "ConfirmedEmail");
            await _userManager.UpdateAsync(user);
        }
    }
}
