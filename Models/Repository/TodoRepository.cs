namespace AuthServer.Models.Repository
{
    public class TodoRepository : GeneralRepository<Todo, UserDbContext>
    {
        public TodoRepository(UserDbContext context) : base(context)
        {
        }
    }
}
