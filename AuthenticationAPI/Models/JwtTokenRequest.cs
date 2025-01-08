namespace AuthenticationAPI.Models
{
    public class JwtTokenRequest
    {
        public string UserID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }
}
