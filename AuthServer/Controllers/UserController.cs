using AuthServer.BLL.Auth;
using AuthServer.BLL.DTO.User;
using AuthServer.BLL.Shared;
using AuthServer.Shared;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AuthServer.Controllers
{
    /// <summary>
    /// Controller for processing users by administrator/system
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        //private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(
            //UserManager<IdentityUser> userManager,
            IMapper mapper, 
            IUserService userService)
        {
            //_userManager = userManager;
            _mapper = mapper;
            _userService = userService;
        }

        [HttpGet("ToDel")]
        public async Task<IActionResult> TodelEmail(string email)
        {

            return Ok();
        }

        /*[HttpGet("ToDel")]
        public async Task<IActionResult> Todel(string name)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:7141");
            var client = new TodoClient.TodoRPC.TodoRPCClient(channel);

            try
            {
                //var reply = await client.SayHelloAsync(new TodoHelloRequest { Name = name });
                //var reply = client.AddTodo(new NewTodo { Title = name, UserId = 4 });
                try
                {
                    //var reply = client.GetTodos(new TodoClient.User { UserId = 6 });
                    var reply = client.AddTodo(new RPCNewTodo { Title = name, UserId = 4 });
                    return Ok(reply);
                }
                catch (RpcException ex) //when (ex.StatusCode == StatusCode.PermissionDenied)
                {
                    var userEntry = ex.Trailers.FirstOrDefault(e => e.Key == "User");
                    Console.WriteLine($"User '{userEntry.Value}' does not have permission to view this portfolio.");
                }
            }
            catch (Exception ex)
            {
                var f = "";
            }

            return BadRequest();
        }*/

        [HttpPost]
        [Authorize(Roles = AppConsts.AdminRole)]
        public async Task<IActionResult> CreateUser(UserPostDto userDto)
        {
            if (userDto == null)
                return BadRequest("No user to update");

            var userAuthDto = _mapper.Map<UserAuthDto>(userDto);

            try
            {
                await _userService.CheckUser(userAuthDto);
                await _userService.RegisterUser(userAuthDto);
            }
            catch (UserInterfaceException ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPatch]
        [Authorize(Roles = AppConsts.AdminRole)]
        public async Task<IActionResult> UpdateUser(UserPatchDto userDto)
        {
            var userAuthDto = _mapper.Map<UserAuthDto>(userDto);
            await _userService.UpdateUser(userAuthDto);
            return Ok();
        }

        [HttpGet()]
        [Authorize(Roles = AppConsts.AdminRole)]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetUsers();
            var usersDto = _mapper.Map<IEnumerable<UserGridBllDto>, IEnumerable<UserGridDto>>(users);
            return Ok(JsonConvert.SerializeObject(usersDto));
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string item, string email)
        {
            if (string.IsNullOrEmpty(email))
                return ShowResult("No email to confirm");

            try
            {
                await _userService.ConfirmEmail(item, email);
            }
            catch (UserInterfaceException ex)
            {
                return ShowResult(ex.Message);
            }

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
