namespace AuthenticationAPI.Tools
{
    public class JwtTokenDefaults
    {
        public string ValidAudience { get; set; }
        public string ValidIssuer { get; set; }
        public string SecretKey { get; set; }
        public int AccessTokenExpires { get; set; }
        public int RefreshTokenExpires { get; set; }
    }
}
