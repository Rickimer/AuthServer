namespace RPC.Shared
{
    public class RPCCreateTodoDto
    {        
        public ulong UserId { get; set; }
        public string? Title { get; set; }
        public bool? IsCompleted { get; set; }
    }
}
