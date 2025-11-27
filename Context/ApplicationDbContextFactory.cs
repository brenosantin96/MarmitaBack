using MarmitaBackend.Provider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Ler appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseMySql(
            configuration.GetConnectionString("DefaultConnection"),
            ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
        );

        // Usar TenantProvider especial de migrations
        return new ApplicationDbContext(optionsBuilder.Options, new DesignTimeTenantProvider());
    }
}
