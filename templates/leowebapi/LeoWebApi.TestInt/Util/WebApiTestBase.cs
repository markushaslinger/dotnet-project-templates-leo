using LeoWebApi.Persistence;
using LeoWebApi.Persistence.Util;

namespace LeoWebApi.TestInt.Util;

public abstract class WebApiTestBase(WebApiTestFixture webApiFixture) : IClassFixture<WebApiTestFixture>, IAsyncLifetime
{
    protected HttpClient ApiClient => webApiFixture.Client;

    public async Task InitializeAsync()
    {
        await webApiFixture.RestoreDatabaseAsync(ImportSeedDataAsync);
    }

    public Task DisposeAsync()
    {
        // nothing to dispose
        // database is reset during init to ensure a clean slate even if a test run is interrupted
        return Task.CompletedTask;
    }

    protected virtual ValueTask ImportSeedDataAsync(DatabaseContext context)
    {
        // add seed data for all tests here if needed, override in derived classes to add test class specific seed data
        return ValueTask.CompletedTask;
    }

    protected async ValueTask ModifyDatabaseContentAsync(Func<DatabaseContext, ValueTask> modifier)
    {
        await webApiFixture.ModifyDatabaseContentAsync(modifier);
    }
}
