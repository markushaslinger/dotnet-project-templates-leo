using LeoWebApi.Persistence;
using LeoWebApi.Persistence.Util;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;

namespace LeoWebApi.TestInt.Util;

internal sealed class WebAppFactory(string connectionString) : WebApplicationFactory<Program>
{
    public IServiceProvider? ServiceProvider { get; private set; }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptorType = typeof(DbContextOptions<DatabaseContext>);

            var descriptor = services
                .SingleOrDefault(s => s.ServiceType == descriptorType);

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<DatabaseContext>(options => PersistenceSetup.ConfigureDatabaseContextOptions(options,
                                                    connectionString, false));

            ServiceProvider = services.BuildServiceProvider();
        });
    }
}
