using LeoWebApi.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace LeoWebApi.Persistence.Repositories;

public interface IPayloadRepository
{
    public ValueTask<Payload?> GetHeaviestPayloadAsync(bool launchedOnly);
}

internal readonly struct PayloadRepository(DbSet<Payload> payloadSet) : IPayloadRepository
{
    public async ValueTask<Payload?> GetHeaviestPayloadAsync(bool launchedOnly)
    {
        var query = payloadSet
                    .Include(p => p.Launch)
                    .ThenInclude(l => l.Rocket)
                    .Where(p => !launchedOnly || p.Launch.LaunchDate != null)
                    .OrderByDescending(p => p.Weight);

        var payload = await query.FirstOrDefaultAsync();

        return payload;
    }
}
