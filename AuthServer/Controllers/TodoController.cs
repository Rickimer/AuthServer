using AuthServer.BLL.DTO.Todo;
using AuthServer.BLL.DTO.User;
using AuthServer.BLL.Shared;
using AuthServer.BLL.Todo;
using AuthServer.Shared.Todo;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace AuthServer.Controllers
{
    //[Authorize(Roles = AppConsts.ConfirmedRole)]
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ToDoController : Controller
    {
        private readonly ITodoBllService _todoService;
        private readonly IMapper _mapper;        
        //private readonly IMemoryCache _cache;

        public ToDoController(IMapper mapper, ITodoBllService todoService
            //, IMemoryCache cache
            )
        {
            _todoService = todoService;
            _mapper = mapper;            
            //_cache = cache;
        }

        //todo:
        //1. Credentials + useiid from +
        //2. Exception handler
        //3. Cash
        //4. TenantID with unique key!
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var tokenInfo = new TokenInfoDto();
            tokenInfo.SetFromClaims(User.Claims);
            var dto = new BLL.DTO.Todo.BllGetTodosDto { UserId = tokenInfo.UserId };
            var result = await _todoService.GetTodosAsync(dto);
            return Ok(JsonConvert.SerializeObject(result));            
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateTodoDto createTodoDto)
        {
            var bllDto = _mapper.Map<BllCreateTodoDto>(createTodoDto);
            var tokenInfo = new TokenInfoDto();
            tokenInfo.SetFromClaims(User.Claims);
            bllDto.UserId = tokenInfo.UserId;

            await _todoService.CreateTodoAsync(bllDto);
            return Ok();
        }

        [HttpPatch]
        public async Task<ActionResult> Update(UpdateTodoDto updateDto)
        {
            var bllDto = _mapper.Map<BllUpdateTodoDto>(updateDto);
            var tokenInfo = new TokenInfoDto();
            tokenInfo.SetFromClaims(User.Claims);
            bllDto.UserId = tokenInfo.UserId;

            await _todoService.UpdateTodoAsync(bllDto);
            return Ok();
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(DeleteTodoDto deleteTodoDto)
        {
            var bllDto = _mapper.Map<BLLTodoDeleteDto>(deleteTodoDto);
            var tokenInfo = new TokenInfoDto();
            tokenInfo.SetFromClaims(User.Claims);
            bllDto.UserId = tokenInfo.UserId;

            await _todoService.DeleteTodoAsync(bllDto);
            return Ok();
        }
    }
}
