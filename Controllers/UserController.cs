using AuthServer.DTO.User;
using AuthServer.Models;
using AuthServer.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager,
            IMapper mapper, IUserService userService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpPost]
        [Authorize(Roles = AppConsts.AppConsts.AdminRole)]
        public async Task<IActionResult> CreateUser(UserPostDto userDto)
        {
            if (userDto == null)
                return BadRequest("No user to update");

            var error = await _userService.CheckUser(userDto);
            if (!String.IsNullOrEmpty(error))
            {
                return Conflict(error);
            }

            await _userService.RegisterUser(userDto);

            return Ok();
        }

        [HttpPatch]
        [Authorize(Roles = AppConsts.AppConsts.AdminRole)]
        public async Task<IActionResult> UpdateUser(UserPatchDto userDto)
        {
            if (userDto == null)
                return BadRequest("No user to update");

            var user = (User)await _userManager.FindByEmailAsync(userDto.Email);
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
                    await _userService.CreateRole(role);
                }
                await _userManager.AddToRolesAsync(user, newRoles);
                await _userManager.RemoveFromRolesAsync(user, rolesToDelete);
            }

            if (!String.IsNullOrEmpty(userDto.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, userDto.Password);
            }
            await _userManager.UpdateAsync(user);
            return Ok();
        }

        [HttpGet()]
        [Authorize(Roles = AppConsts.AppConsts.AdminRole)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var usersDto = new List<UserGridDto>();

            int i = 0;
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (user.EmailConfirmed)
                {
                    roles.Add("ConfirmedEmail");
                }
                var userDto = new UserGridDto
                {
                    UserName = user.UserName,
                    Roles = roles,
                    Email = user.Email,
                    Id = i
                };

                usersDto.Add(userDto);
                i++;
            }

            return Ok(JsonConvert.SerializeObject(usersDto));
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string item, string email)
        {
            if (string.IsNullOrEmpty(email))
                return ShowResult("No email to confirm");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception("User not found!");

            if (user.EmailConfirmed)
                return ShowResult($"User email {email} already confirmed!");

            var hash = item;

            if (user.PasswordHash != hash)
            {
                return ShowResult($"Not a valid user identification!");
            }

            user.EmailConfirmed = true;
            await _userService.CreateRole("ConfirmedEmail");
            await _userManager.AddToRoleAsync(user, "ConfirmedEmail");
            await _userManager.UpdateAsync(user);

            return ShowResult("Success confirm");
        }

        private IActionResult ShowResult(string message)
        {
            var result = new ContentResult
            {
                Content = $"<p>{message}</p>",
                ContentType = "text/html"
            };

            return result;
        }
    }
}
