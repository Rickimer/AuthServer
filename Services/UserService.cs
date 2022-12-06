using AuthServer.DTO.User;
using AuthServer.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace AuthServer.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public UserService(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,            
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task CreateRole(string roleName)
        {
            if (!(await _roleManager.RoleExistsAsync(roleName)))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }            
        }

        public async Task<string> CheckUser(UserAuthDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
            {
                return "This email is already registered";
            }

            var userName = await _userManager.FindByNameAsync(dto.UserName);
            if (userName != null)
            {
                return $"username {userName} is already registered";
            }

            return String.Empty;
        }

        public async Task<UserDto> RegisterUser(UserAuthDto dto)
        {
            if (dto.Password == null)
            {
                throw new ArgumentException("password");
            }

            var user = new User { UserName = dto.UserName ?? dto.Email, Email = dto.Email, Created = DateTime.Now, EmailConfirmed = false };
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                throw new Exception("Error creating user"+result.Errors);                
            }

            if (dto.Roles != null)
            {
                if (dto.Roles.Contains("ConfirmedEmail"))
                {
                    dto.Roles.Remove("ConfirmedEmail");
                    user.EmailConfirmed = true;
                }
                else
                {
                    user.EmailConfirmed = false;
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

            if (!String.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, dto.Password);
            }
            await _userManager.UpdateAsync(user);

            var userDto = _mapper.Map<UserDto>(user);
            return userDto;
        }
    }
}
