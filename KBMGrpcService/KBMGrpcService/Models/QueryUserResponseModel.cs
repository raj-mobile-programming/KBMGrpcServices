namespace KBMGrpcService.Models
{
    public class QueryUsersResponseModel
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int Total { get; set; }
        public IEnumerable<UsersModel> Users { get; set; } = Enumerable.Empty<UsersModel>();
    }

}
