using Grpc.Core;
using KBMGrpcService.Protos;

namespace KBMGrpcService.Services
{
    public class UserService : KBMGrpcService.Protos.UserService.UserServiceBase
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Username and Email are required."));
                }

                if (!IsValidEmail(request.Email))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid email format."));
                }

                var existingUser = _context.Users
                    .FirstOrDefault(u => u.Username == request.Username || u.Email == request.Email && !u.IsDeleted);
                if (existingUser != null)
                {
                    throw new RpcException(new Status(StatusCode.AlreadyExists, "User with this username or email already exists."));
                }

                var user = new Protos.UserModel
                {
                    Name = request.Name,
                    Username = request.Username,
                    Email = request.Email,
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return new CreateUserResponse { UserId = user.Id };
            }
            catch(RpcException ex)
            {
                return null;
            }
        }

        public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
            }

            return new GetUserResponse
            {
                Name = user.Name,
                Username = user.Username,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }
        //Email validation
        private bool IsValidEmail(string email)
        {
            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}