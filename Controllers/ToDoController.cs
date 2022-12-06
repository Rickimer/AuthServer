using AuthServer.DTO;
using AuthServer.Models;
using AuthServer.Models.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace AuthServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ToDoController : Controller
    {
        private readonly IMapper _mapper;
        private readonly TodoRepository _repository;

        public ToDoController(IMapper mapper 
            , TodoRepository repository
            )
        {            
            _mapper = mapper;
            _repository = repository;
        }

        [HttpGet]
        [Authorize(Roles = AppConsts.AppConsts.ConfirmedRole)]
        public async Task<ActionResult> Index()
        {
           var list = await _repository.GetAllAsync();
            var result = list.Select(e => _mapper.Map<TodoDto>(e));

            return Ok(JsonConvert.SerializeObject(result));
        }

        [HttpPost]
        [Authorize(Roles = AppConsts.AppConsts.ConfirmedRole)]
        public async Task<ActionResult> Create(TodoDto dto)
        {            
            var todo = _mapper.Map<Todo>(dto);
            await _repository.AddAsync(todo);
            return Ok();
        }

        [HttpPatch]
        [Authorize(Roles = AppConsts.AppConsts.ConfirmedRole)]
        public async Task<ActionResult> Update(TodoDto updateDto)
        {
            var todo = _mapper.Map<Todo>(updateDto);
            await _repository.UpdateAsync(todo);
            return Ok();
        }

        [HttpDelete]
        [Authorize(Roles = AppConsts.AppConsts.ConfirmedRole)]
        public async Task<ActionResult> Delete(DeleteDto deleteDto)            
        {
            await _repository.Delete(deleteDto.Id);
            return Ok();
        }
    }
}
