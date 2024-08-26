using Grpc.Core;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;

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
            catch (RpcException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Status.Detail));
            }
        }

        public override async Task<GetUserResponse> GetUser(GetUserRequest request, ServerCallContext context)
        {
            try
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
            catch (RpcException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Status.Detail));
            }
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
        //Query User
        public override async Task<QueryUsersResponse> QueryUsers(QueryUsersRequest request, ServerCallContext context)
        {
            try
            {
                var query = _context.Users.Where(u => !u.IsDeleted);

                if (!string.IsNullOrWhiteSpace(request.QueryString))
                {
                    query = query.Where(u => u.Name.Contains(request.QueryString) || u.Username.Contains(request.QueryString) || u.Email.Contains(request.QueryString));
                }

                var total = await query.CountAsync();
                var users = await query
                    .OrderBy(u => request.OrderBy == "Name" ? u.Name : u.CreatedAt.ToString())
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(u => new UserModel
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Username = u.Username,
                        Email = u.Email,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();

                return new QueryUsersResponse
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Total = total,
                    Users = { users }
                };
            }
            catch (RpcException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Status.Detail));
            }
        }

        public async override Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            try
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null || user.IsDeleted)
                {
                    throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
                }

                if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Username and Email are required."));
                }

                if (!IsValidEmail(request.Email))
                {
                    throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid email format."));
                }

                user.Name = request.Name;
                user.Username = request.Username;
                user.Email = request.Email;
                user.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return new UpdateUserResponse { Message = "User updated successfully." };
            }
            catch (RpcException ex)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Status.Detail));
            }
        }

        public async override Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null || user.IsDeleted)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found."));
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return new DeleteUserResponse { Message = "User deleted successfully." };
        }

        //Associate user with organization
        public async override Task<AssociateUserToOrganizationResponse> AssociateUserToOrganization(AssociateUserToOrganizationRequest request, ServerCallContext context)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == request.UserId && !user.IsDeleted);
                var organization = await _context.Organizations.FirstOrDefaultAsync(options => options.Id == request.OrganizationId && !options.IsDeleted);

                if (user == null || organization == null)
                {
                    return new AssociateUserToOrganizationResponse
                    {
                        Message = "User or Organization not found."
                    };
                }

                user.OrganizationId = request.OrganizationId;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return new AssociateUserToOrganizationResponse { Message = "User associated successfully." };
            }
            catch (Exception ex)
            {
                return new AssociateUserToOrganizationResponse { Message = "User association to organization failed." };
            }
        }

        //Disassociate user from organization
        public async override Task<DisassociateUserFromOrganizationResponse> DisassociateUserFromOrganization(DisassociateUserFromOrganizationRequest request, ServerCallContext context)
        {
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == request.UserId && !user.IsDeleted);
            if (user == null)
            {
                return new DisassociateUserFromOrganizationResponse { Message = "User not found." };
            }

            user.OrganizationId = 0; //Updating organization Id here as 0
            _context.SaveChangesAsync();

            return new DisassociateUserFromOrganizationResponse { Message = "User disassociated successfully" };
        }
    }
}