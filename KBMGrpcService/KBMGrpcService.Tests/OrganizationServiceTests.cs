using Xunit;
using Moq;
using KBMGrpcService.Protos;
using Grpc.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

public class OrganizationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly OrganizationService _service;

    public OrganizationServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "OrganizationServiceTests")
            .Options;
        _context = new AppDbContext(options);
        _service = new OrganizationService(_context);
    }

    [Fact]
    public async Task CreateOrganization_ValidRequest_ReturnsOrganizationId()
    {
        // Arrange
        var request = new CreateOrganizationRequest { Name = "New Organization", Address = "123 Main St" };

        // Act
        var response = await _service.CreateOrganization(request, null);

        // Assert
        Assert.NotNull(response);
        Assert.True(response.OrganizationId > 0);
    }

    [Fact]
    public async Task CreateOrganization_MissingName_ThrowsRpcException()
    {
        // Arrange
        var request = new CreateOrganizationRequest { Name = "", Address = "123 Main St" };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _service.CreateOrganization(request, null));
    }

    [Fact]
    public async Task CreateOrganization_DuplicateName_ThrowsRpcException()
    {
        // Arrange
        var existingOrg = new OrganizationModel { Name = "Existing Org", Address = "123 Main St", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
        _context.Organizations.Add(existingOrg);
        await _context.SaveChangesAsync();

        var request = new CreateOrganizationRequest { Name = "Existing Org", Address = "123 Another St" };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _service.CreateOrganization(request, null));
    }

    [Fact]
    public async Task GetOrganization_ValidId_ReturnsOrganization()
    {
        // Arrange
        var organization = new OrganizationModel { Name = "Test Org", Address = "123 Main St", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        var request = new GetOrganizationRequest { Id = organization.Id };

        // Act
        var response = await _service.GetOrganization(request, null);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(organization.Name, response.Name);
    }

    [Fact]
    public async Task GetOrganization_InvalidId_ThrowsRpcException()
    {
        // Arrange
        var request = new GetOrganizationRequest { Id = 999 };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _service.GetOrganization(request, null));
    }

    [Fact]
    public async Task UpdateOrganization_ValidRequest_ReturnsSuccessMessage()
    {
        // Arrange
        var organization = new OrganizationModel { Name = "Original Name", Address = "123 Main St", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        var request = new UpdateOrganizationRequest { OrganizationId = organization.Id, Name = "Updated Name", Address = "456 Another St" };

        // Act
        var response = await _service.UpdateOrganization(request, null);

        // Assert
        Assert.Equal("Organization updated successfully.", response.Message);
    }

    [Fact]
    public async Task UpdateOrganization_InvalidId_ThrowsRpcException()
    {
        // Arrange
        var request = new UpdateOrganizationRequest { OrganizationId = 999, Name = "Updated Name", Address = "456 Another St" };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _service.UpdateOrganization(request, null));
    }

    [Fact]
    public async Task DeleteOrganization_ValidRequest_ReturnsSuccessMessage()
    {
        // Arrange
        var organization = new OrganizationModel { Name = "Test Org", Address = "123 Main St", CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() };
        _context.Organizations.Add(organization);
        await _context.SaveChangesAsync();

        var request = new DeleteOrganizationRequest { OrganizationId = organization.Id };

        // Act
        var response = await _service.DeleteOrganization(request, null);

        // Assert
        Assert.Equal("Organization deleted successfully.", response.Message);
    }

    [Fact]
    public async Task DeleteOrganization_InvalidId_ThrowsRpcException()
    {
        // Arrange
        var request = new DeleteOrganizationRequest { OrganizationId = 999 };

        // Act & Assert
        await Assert.ThrowsAsync<RpcException>(() => _service.DeleteOrganization(request, null));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
