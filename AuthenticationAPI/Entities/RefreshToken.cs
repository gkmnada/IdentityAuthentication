namespace AuthenticationAPI.Entities
{
    public class RefreshToken
    {
        public int RefreshTokenID { get; set; }
        public string Token { get; set; }
        public string UserID { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsRevoked { get; set; }
    }
}
