using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<KBMGrpcService.Protos.OrganizationModel> Organizations { get; set; }
    public DbSet<KBMGrpcService.Protos.UserModel> Users { get; set; }
}
