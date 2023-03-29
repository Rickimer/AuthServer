using RPC.Shared;

namespace RPC
{
    public interface ITodoRPCService
    {
        Task<RpcCreateResultDto> CreateTodoAsync(RPCCreateTodoDto dto);
        Task DeleteTodoAsync(RPCDeleteTodoDto dto);
        Task UpdateTodoAsync(RPCUpdateTodoDto dto);
        Task<IEnumerable<RPCTodoDto>> GetTodosAsync(RPCGetTodosInputDto dto);
    }
}
