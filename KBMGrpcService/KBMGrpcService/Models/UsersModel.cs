namespace KBMGrpcService.Models
{
    public class UsersModel
    {
            public long Id { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public long? OrganizationId { get; set; }
            public long CreatedAt { get; set; }
            public long? UpdatedAt { get; set; }
            public long? DeletedAt { get; set; }
            public bool IsDeleted { get; set; }
    }
}
