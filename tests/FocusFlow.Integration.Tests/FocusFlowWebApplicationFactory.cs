using FocusFlow.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FocusFlow.Integration.Tests;

public class FocusFlowWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "DataSource=:memory:");
        builder.UseSetting("JwtSettings:SecretKey", "SuperSecretKeyForIntegrationTesting12345!@#");
        builder.UseSetting("JwtSettings:Issuer", "FocusFlowTest");
        builder.UseSetting("JwtSettings:Audience", "FocusFlowTest");
        builder.UseSetting("JwtSettings:ExpirationHours", "24");

        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext registration
            services.RemoveAll(typeof(DbContextOptions<FocusFlowDbContext>));

            // Register a new DbContext using the InMemory provider
            services.AddDbContext<FocusFlowDbContext>(options =>
            {
                options.UseInMemoryDatabase("FocusFlowIntegrationTestDb");
            });
        });
    }
}
