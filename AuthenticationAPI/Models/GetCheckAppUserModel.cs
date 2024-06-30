namespace AuthenticationAPI.Models
{
    public class GetCheckAppUserModel
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public bool IsExist { get; set; }
    }
}
