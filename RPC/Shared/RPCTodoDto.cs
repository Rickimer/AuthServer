namespace RPC.Shared
{
    public class RPCTodoDto
    {
        public ulong Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool isCompleted { get; set; }
    }
}
