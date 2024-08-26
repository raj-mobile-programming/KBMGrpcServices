using Xunit;
using Microsoft.EntityFrameworkCore;
using KBMGrpcService.Protos;
using KBMGrpcService.Services;
using System.Threading.Tasks;
using Grpc.Core;

public class OrganizationServiceIntegrationTests
{
    private readonly OrganizationService _organizationService;
    private readonly AppDbContext _context;

    public OrganizationServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new AppDbContext(options);
        _organizationService = new OrganizationService(_context);
    }

    [Fact]
    public async Task CreateOrganization_IntegrationTest()
    {
        // Arrange
        var request = new CreateOrganizationRequest { Name = "Integration Test Org", Address = "Integration Address" };

        // Act
        var response = await _organizationService.CreateOrganization(request, null);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.OrganizationId > 0);
    }

    [Fact]
    public async Task GetOrganization_IntegrationTest()
    {
        // Arrange
        var organization = new OrganizationModel
        {
            Name = "Test Org",
            Address = "Test Address",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        var request = new GetOrganizationRequest { Id = organization.Id };

        // Act
        var response = await _organizationService.GetOrganization(request, null);

        // Assert
        Assert.Equal(organization.Name, response.Name);
    }
    

    [Fact]
    public async Task UpdateOrganization_IntegrationTest()
    {
        // Arrange
        var organization = new OrganizationModel
        {
            Name = "Org to Update",
            Address = "Old Address",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        var request = new UpdateOrganizationRequest
        {
            OrganizationId = organization.Id,
            Name = "Updated Org",
            Address = "New Address"
        };

        // Act
        var response = await _organizationService.UpdateOrganization(request, null);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Organization updated successfully.", response.Message);

        var updatedOrganization = await _context.Organizations.FindAsync(organization.Id);
        Assert.Equal("Updated Org", updatedOrganization.Name);
        Assert.Equal("New Address", updatedOrganization.Address);
    }
    [Fact]
    public async Task DeleteOrganization_IntegrationTest()
    {
        // Arrange
        var organization = new OrganizationModel
        {
            Name = "Org to Delete",
            Address = "Address",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        var request = new DeleteOrganizationRequest
        {
            OrganizationId = organization.Id
        };

        // Act
        var response = await _organizationService.DeleteOrganization(request, null);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Organization deleted successfully.", response.Message);

        var deletedOrganization = await _context.Organizations.FindAsync(organization.Id);
        Assert.True(deletedOrganization.IsDeleted);
        Assert.True(deletedOrganization.DeletedAt > 0);
    }
    [Fact]
    public async Task GetDeletedOrganization_IntegrationTest()
    {
        // Arrange
        var organization = new OrganizationModel
        {
            Name = "Deleted Org",
            Address = "Deleted Address",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            IsDeleted = true,
            DeletedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        var request = new GetOrganizationRequest
        {
            Id = organization.Id
        };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _organizationService.GetOrganization(request, null));
    }
    [Fact]
    public async Task CreateDuplicateOrganization_IntegrationTest()
    {
        // Arrange
        var organization = new OrganizationModel
        {
            Name = "Duplicate Org",
            Address = "Address",
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        var request = new CreateOrganizationRequest
        {
            Name = "Duplicate Org",
            Address = "New Address"
        };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _organizationService.CreateOrganization(request, null));
    }
    [Fact]
    public async Task UpdateNonExistentOrganization_IntegrationTest()
    {
        // Arrange
        var request = new UpdateOrganizationRequest
        {
            OrganizationId = 999,  // Non-existent ID
            Name = "Non-existent Org",
            Address = "Address"
        };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _organizationService.UpdateOrganization(request, null));
    }
[Fact]
public async Task DeleteNonExistentOrganization_IntegrationTest()
{
    // Arrange
    var request = new DeleteOrganizationRequest
    {
        OrganizationId = 999  // Non-existent ID
    };

    // Act & Assert
    await Assert.ThrowsAsync<RpcException>(() => _organizationService.DeleteOrganization(request, null));
}

}
