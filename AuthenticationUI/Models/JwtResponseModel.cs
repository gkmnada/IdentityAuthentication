namespace AuthenticationUI.Models
{
    public class JwtResponseModel
    {
        public string Token { get; set; }
        public DateTime ExpireDate { get; set; }
        public string RefreshToken { get; set; }
    }
}
