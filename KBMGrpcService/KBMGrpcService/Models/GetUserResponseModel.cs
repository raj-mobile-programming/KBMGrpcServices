namespace KBMGrpcService.Models
{
    public class GetUserResponseModel
    {
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public long CreatedAt { get; set; }
    }

}
