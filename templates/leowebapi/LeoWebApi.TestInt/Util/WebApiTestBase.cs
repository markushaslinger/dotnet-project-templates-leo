using LeoWebApi.Persistence;

namespace LeoWebApi.TestInt.Util;

public abstract class WebApiTestBase(WebApiTestFixture webApiFixture) : IClassFixture<WebApiTestFixture>, IAsyncLifetime
{
    protected HttpClient ApiClient => webApiFixture.Client;

    public async Task InitializeAsync()
    {
        await webApiFixture.RestoreDatabase(ImportSeedData);
    }

    public Task DisposeAsync()
    {
        // nothing to dispose
        // database is reset during init to ensure a clean slate even if a test run is interrupted
        return Task.CompletedTask;
    }

    protected virtual ValueTask ImportSeedData(DatabaseContext context)
    {
        // add seed data for all tests here if needed, override in derived classes to add test class specific seed data
        return ValueTask.CompletedTask;
    }

    protected async ValueTask ModifyDatabaseContent(Func<DatabaseContext, ValueTask> modifier)
    {
        await webApiFixture.ModifyDatabaseContent(modifier);
    }
}