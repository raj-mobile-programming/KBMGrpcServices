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
        catch(RpcException ex)
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

}
