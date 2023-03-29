using AuthServer.BLL.DTO.Todo;


namespace AuthServer.BLL.Todo
{
    public interface ITodoBllService
    {
        public Task CreateTodoAsync(BllCreateTodoDto dto);
        public Task UpdateTodoAsync(BllUpdateTodoDto dto);
        public Task DeleteTodoAsync(BLLTodoDeleteDto dto);
        public Task<IEnumerable<BllTodoDto>> GetTodosAsync(BllGetTodosDto dto);
    }
}
