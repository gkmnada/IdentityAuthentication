namespace AuthenticationAPI.Tools
{
    public class JwtTokenDefaults
    {
        public const string ValidAudience = "https://localhost";
        public const string ValidIssuer = "https://localhost";
        public const string SecretKey = "IdentityAuthentication-Project-Designed-By-Gokmen-Ada-17081999-Jwt-Security-Identity-Security-30062024";
        public const int AccessTokenExpiration = 15;
        public const int RefreshTokenExpiration = 7;

    }
}
