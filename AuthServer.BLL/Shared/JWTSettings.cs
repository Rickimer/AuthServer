namespace AuthServer.BLL.Shared
{
    public class JWTSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int TokenMinutesLifeTime { get; set; }
        public int RefreshTokenDaysLifeTime { get; set; }
    }
}
