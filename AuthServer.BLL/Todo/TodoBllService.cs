using AuthServer.BLL.Auth;
using AuthServer.BLL.DTO.User;
using AuthServer.DAL.Data.Repository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TodoClient;
using AuthServer.BLL.DTO.Todo;
using RPC;
using RPC.Shared;
using AuthServer.BLL.Todo;
using Microsoft.Extensions.Caching.Memory;

namespace AuthServer.BLL.DTO.Todo
{
    public class TodoBllService : ITodoBllService
    {
        private readonly IMapper _mapper;        
        private readonly ITodoRPCService _todoRPCService;
        private readonly IMemoryCache _cache;
        
        public TodoBllService(IMapper mapper, ITodoRPCService todoRPCService, IMemoryCache cache)
        {
            _mapper = mapper;
            _todoRPCService = todoRPCService;
            _cache = cache;
        }

        public async Task CreateTodoAsync(BllCreateTodoDto dto)
        {            
            var rpcDto = _mapper.Map<RPCCreateTodoDto>(dto);
            var result = await _todoRPCService.CreateTodoAsync(rpcDto);            
            if (result.Id > 0)
            {
                _cache.Remove(dto.UserId);
            }
        }

        public async Task UpdateTodoAsync(BllUpdateTodoDto dto)
        {
            var rpcDto = _mapper.Map<RPCUpdateTodoDto>(dto);
            await _todoRPCService.UpdateTodoAsync(rpcDto);
            _cache.Remove(dto.UserId);
        }

        public async Task DeleteTodoAsync(BLLTodoDeleteDto dto)
        {
            var rpcDto = _mapper.Map<RPCDeleteTodoDto>(dto);
            await _todoRPCService.DeleteTodoAsync(rpcDto);
            _cache.Remove(dto.UserId);
        }
        
        public async Task<IEnumerable<BllTodoDto>> GetTodosAsync(BllGetTodosDto dto)
        {
            IEnumerable<BllTodoDto>? userTodos;
            if (!_cache.TryGetValue(dto.UserId, out userTodos))
            {
                var rpcDto = _mapper.Map<RPCGetTodosInputDto>(dto);
                var res = (await _todoRPCService.GetTodosAsync(rpcDto)).ToList();
                userTodos = _mapper.Map<List<RPCTodoDto>, List<BllTodoDto>>(res);

                if (userTodos != null)
                {
                    _cache.Set(dto.UserId, userTodos,
                        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
                }                
            }

            if (userTodos == null)
                userTodos = new List<BllTodoDto>();
            return userTodos;
        }
    }
}