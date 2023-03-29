using AutoMapper;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using RPC.Shared;
using TodoClient;

namespace RPC
{
    public class TodoRPCService : ITodoRPCService
    {
        private readonly ILogger<TodoRPCService> _logger;
        private readonly IMapper _mapper;
        private readonly TodoRPC.TodoRPCClient _rpcTodoClient;

        public TodoRPCService(ILogger<TodoRPCService> logger, IMapper mapper, TodoRPC.TodoRPCClient rpcTodoClient)
        {
            _logger = logger;
            _mapper = mapper;
            _rpcTodoClient = rpcTodoClient;
        }

        public async Task<RpcCreateResultDto> CreateTodoAsync(RPCCreateTodoDto dto)
        {
            var rpcDto = _mapper.Map<TodoClient.RPCCreateTodo>(dto);
            var result = await _rpcTodoClient.CreateTodoAsync(rpcDto);
            var res = _mapper.Map<RpcCreateResultDto>(result);
            return res;
        }

        public async Task DeleteTodoAsync(RPCDeleteTodoDto dto)
        {
            var rpcDto = _mapper.Map<TodoClient.RPCDeleteTodo>(dto);
            await _rpcTodoClient.DeleteTodoAsync(rpcDto);
        }

        public async Task UpdateTodoAsync(RPCUpdateTodoDto dto)
        {
            var rpcDto = _mapper.Map<TodoClient.RPCUpdateTodo>(dto);
            await _rpcTodoClient.UpdateTodoAsync(new RPCUpdateTodo { Id = dto.Id, Title = dto.Title });
        }

        public async Task<IEnumerable<RPCTodoDto>> GetTodosAsync(RPCGetTodosInputDto dto)
        {
            try
            {
                var rpcDto = _mapper.Map<TodoClient.RPCUser>(dto);
                var r1 = await _rpcTodoClient.GetTodosAsync(rpcDto);
                var rpcresult = r1.Todos.ToList();
                
                var result = _mapper.Map<List<TodoClient.RPCTodo>, List<RPCTodoDto>>(rpcresult);
                return result;
            }
            catch (Exception ex)
            {
                var f2 = ex.Message;
            }

            return new List<RPCTodoDto>();
        }
    }
}
