using LeoWebApi.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace LeoWebApi.TestInt.Util;

// ReSharper disable once ClassNeverInstantiated.Global - Instantiated by xUnit
public sealed class WebApiTestFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
                                                              .WithImage("postgres:17")
                                                              .WithDatabase("public")
                                                              .WithUsername("postgres")
                                                              .WithPassword("postgres")
                                                              .Build();

    private HttpClient? _client;
    private WebAppFactory? _factory;

    public HttpClient Client
    {
        get => _client ?? throw new InvalidOperationException("Client not created");
        private set => _client = value;
    }

    private WebAppFactory Factory
    {
        get => _factory ?? throw new InvalidOperationException("Factory not created");
        set => _factory = value;
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        _factory = new WebAppFactory(_postgresContainer.GetConnectionString());
        Client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.StopAsync();
        await _postgresContainer.DisposeAsync();
    }

    public async ValueTask RestoreDatabase(Func<DatabaseContext, ValueTask> seedDataImporter)
    {
        await using var contextScope = CreateContextScope();

        // Drop schema, if it exists
        await contextScope.Context.Database
                          .ExecuteSqlRawAsync($"DROP SCHEMA IF EXISTS \"{DatabaseContext.SchemaName}\" CASCADE;");

        // Apply migrations and ensure the database is created
        await contextScope.Context.Database.MigrateAsync();

        await seedDataImporter(contextScope.Context);
    }

    public async ValueTask ModifyDatabaseContent(Func<DatabaseContext, ValueTask> modifier)
    {
        await using var contextScope = CreateContextScope();

        await modifier(contextScope.Context);
    }

    private ContextScope CreateContextScope()
    {
        var serviceProvider = Factory.ServiceProvider;
        if (serviceProvider is null)
        {
            throw new InvalidOperationException("Service provider not set");
        }

        return new ContextScope(serviceProvider);
    }

    private sealed class ContextScope : IAsyncDisposable
    {
        private readonly IServiceScope _scope;

        public ContextScope(IServiceProvider serviceProvider)
        {
            _scope = serviceProvider.CreateScope();
            Context = _scope.ServiceProvider.GetRequiredService<DatabaseContext>();
        }

        public DatabaseContext Context { get; }

        public async ValueTask DisposeAsync()
        {
            if (_scope is IAsyncDisposable scopeAsyncDisposable)
            {
                await scopeAsyncDisposable.DisposeAsync();
            }
            else
            {
                _scope.Dispose();
            }

            await Context.DisposeAsync();
        }
    }
}