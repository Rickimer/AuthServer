namespace AuthServer.BLL.Shared
{
    public class UserInterfaceException : Exception
    {
        public UserInterfaceException(string message)
        : base(message) { }
    }
}
