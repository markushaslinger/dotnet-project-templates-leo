using LeoWebApi.Persistence.Model;
using LeoWebApi.Shared;
using Microsoft.EntityFrameworkCore;

namespace LeoWebApi.Persistence.Repositories;

public interface ILaunchRepository
{
    public Launch AddLaunch(LocalDate plannedLaunchDate, string launchSite, string customer, Rocket rocket);
    public ValueTask<IReadOnlyCollection<(Guid Id, LocalDate PlannedDate)>> GetLaunchesWithLaunchDateAfterAsync(LocalDate thresholdDate);
    public ValueTask<Launch?> GetLaunchByIdAsync(Guid launchId, bool tracking);
}

internal readonly struct LaunchRepository(DbSet<Launch> launchSet) : ILaunchRepository
{
    private IQueryable<Launch> Launches => launchSet;
    private IQueryable<Launch> LaunchesNoTracking => Launches.AsNoTracking();

    public Launch AddLaunch(LocalDate plannedLaunchDate, string launchSite, string customer, Rocket rocket)
    {
        var launch = new Launch
        {
            PlannedLaunchDate = plannedLaunchDate,
            LaunchSite = launchSite,
            Customer = customer,
            Rocket = rocket,
            Payloads = []
        };

        launchSet.Add(launch);

        return launch;
    }

    public async ValueTask<IReadOnlyCollection<(Guid Id, LocalDate PlannedDate)>> GetLaunchesWithLaunchDateAfterAsync(
        LocalDate thresholdDate)
    {
        var thresholdInstant = thresholdDate.ToInstantInZone();
        var query = LaunchesNoTracking
                    .Where(l => (l.LaunchDate != null && l.LaunchDate.Value > thresholdInstant)
                                || (l.LaunchDate == null && l.PlannedLaunchDate > thresholdDate))
                    .Select(l=>  new
                    {
                        l.Id,
                        l.PlannedLaunchDate
                    })
                    .OrderBy(l => l.PlannedLaunchDate);

        var launches = await query.ToListAsync();

        return launches
               .Select(a => (a.Id, a.PlannedLaunchDate))
               .ToList();
    }

    public async ValueTask<Launch?> GetLaunchByIdAsync(Guid launchId, bool tracking)
    {
        var source = tracking ? Launches : LaunchesNoTracking;
        var launch = await source
                           .Include(l => l.Rocket)
                           .Include(l => l.Payloads)
                           .FirstOrDefaultAsync(l => l.Id == launchId);

        return launch;
    }
}
