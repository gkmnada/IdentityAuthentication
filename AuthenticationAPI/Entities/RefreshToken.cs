namespace AuthenticationAPI.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime Expiration { get; set; }
        public bool IsRevoked { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
    }
}
