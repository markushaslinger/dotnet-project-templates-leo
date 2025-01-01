using LeoWebApi.Persistence.Model;
using Microsoft.EntityFrameworkCore;

namespace LeoWebApi.Persistence.Repositories;

public interface IRocketRepository
{
    public Rocket AddRocket(string modelName, string manufacturer, double maxThrust, long totalDeltaV);
    public ValueTask<IReadOnlyCollection<Rocket>> GetAllRocketsAsync(bool launchedOnly, bool tracking);
    public ValueTask<Rocket?> GetRocketByIdAsync(int id, bool tracking);
    public void RemoveRocket(Rocket rocket);
}

internal readonly struct RocketRepository(DbSet<Rocket> rocketSet) : IRocketRepository
{
    private IQueryable<Rocket> Rockets => rocketSet;
    private IQueryable<Rocket> RocketsNoTracking => Rockets.AsNoTracking();

    public Rocket AddRocket(string modelName, string manufacturer, double maxThrust, long totalDeltaV)
    {
        var rocket = new Rocket
        {
            ModelName = modelName,
            Manufacturer = manufacturer,
            MaxThrust = maxThrust,
            PayloadDeltaV = totalDeltaV
        };

        rocketSet.Add(rocket);

        return rocket;
    }

    public async ValueTask<IReadOnlyCollection<Rocket>> GetAllRocketsAsync(bool launchedOnly, bool tracking)
    {
        var source = tracking ? Rockets : RocketsNoTracking;
        var query = source;

        if (launchedOnly)
        {
            query = query.Where(r => r.Launch != null && r.Launch.LaunchDate != null);
        }

        var rockets = await query.ToListAsync();

        return rockets;
    }

    public async ValueTask<Rocket?> GetRocketByIdAsync(int id, bool tracking)
    {
        var source = tracking ? Rockets : RocketsNoTracking;
        var query = source
                    // bang here is ok, EF ensures that no include happens if the optional FK is null
                    .Include(r => r.Launch!)
                    .ThenInclude(l => l.Payloads);

        var rocket = await query.FirstOrDefaultAsync(r => r.Id == id);

        return rocket;
    }

    public void RemoveRocket(Rocket rocket)
    {
        rocketSet.Remove(rocket);
    }
}
