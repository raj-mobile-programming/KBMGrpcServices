using Grpc.Core;
using KBMGrpcService.Protos;
using Microsoft.EntityFrameworkCore;

public class OrganizationService : KBMGrpcService.Protos.OrganizationService.OrganizationServiceBase
{
    private readonly AppDbContext _context;

    public OrganizationService(AppDbContext context)
    {
        _context = context;
    }

    public override async Task<CreateOrganizationResponse> CreateOrganization(CreateOrganizationRequest request, ServerCallContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Organization name is required."));
            }

            var existingOrg = _context.Organizations
                .FirstOrDefault(options => options.Name == request.Name && !options.IsDeleted);
            if (existingOrg != null)
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Organization with this name already exists."));
            }

            var organization = new KBMGrpcService.Protos.OrganizationModel
            {
                Name = request.Name,
                Address = request.Address,
                CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };

            _context.Organizations.Add(organization);
            await _context.SaveChangesAsync();

            return new CreateOrganizationResponse { OrganizationId = organization.Id };
        }
        catch (RpcException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Status.Detail));
        }
    }

    public override async Task<GetOrganizationResponse> GetOrganization(GetOrganizationRequest request, ServerCallContext context)
    {
        try
        {
            var organization = await _context.Organizations.FindAsync(request.Id);
            if (organization == null || organization.IsDeleted)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Organization not found."));
            }

            return new GetOrganizationResponse
            {
                Name = organization.Name,
                Address = organization.Address,
                CreatedAt = organization.CreatedAt
            };
        }
        catch (RpcException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Status.Detail));
        }
    }
    //Query Organization
    public override async Task<QueryOrganizationsResponse> QueryOrganizations(QueryOrganizationsRequest request, ServerCallContext context)
    {
        try
        {
            var query = _context.Organizations.Where(o => !o.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.QueryString))
            {
                query = query.Where(o => o.Name.Contains(request.QueryString) || o.Address.Contains(request.QueryString));
            }

            var total = await query.CountAsync();
            var organizations = await query
                .OrderBy(o => request.OrderBy == "Name" ? o.Name : o.CreatedAt.ToString())
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(o => new OrganizationModel
                {
                    Id = o.Id,
                    Name = o.Name,
                    Address = o.Address,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync();

            return new QueryOrganizationsResponse
            {
                Page = request.Page,
                PageSize = request.PageSize,
                Total = total,
                Organizations = { organizations }
            };
        }
        catch (RpcException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Status.Detail));
        }
    }
    public async override Task<UpdateOrganizationResponse> UpdateOrganization(UpdateOrganizationRequest request, ServerCallContext context)
    {
        try
        {
            var organization = await _context.Organizations.FindAsync(request.OrganizationId);
            if (organization == null || organization.IsDeleted)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Organization not found."));
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Organization name is required."));
            }

            organization.Name = request.Name;
            organization.Address = request.Address;
            organization.UpdatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync();

            return new UpdateOrganizationResponse { Message = "Organization updated successfully." };
        }
        catch (RpcException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Status.Detail));
        }
    }


    public async override Task<DeleteOrganizationResponse> DeleteOrganization(DeleteOrganizationRequest request, ServerCallContext context)
    {
        try
        {
            var organization = await _context.Organizations.FindAsync(request.OrganizationId);
            if (organization == null || organization.IsDeleted)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Organization not found."));
            }

            organization.IsDeleted = true;
            organization.DeletedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            _context.Organizations.Update(organization);
            await _context.SaveChangesAsync();

            return new DeleteOrganizationResponse { Message = "Organization deleted successfully." };
        }
        catch (RpcException ex)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Status.Detail));
        }
    }
}
