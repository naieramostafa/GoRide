using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RideSharing.Application.Interfaces;
using RideSharing.Core.Enums;

namespace RideSharing.Infrastructure.BackgroundJobs;

public class RideMatchingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RideMatchingBackgroundService> _logger;

    public RideMatchingBackgroundService(IServiceProvider sp, ILogger<RideMatchingBackgroundService> logger)
    {
        _serviceProvider = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Ride matching service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var unassignedRides = await uow.Rides.GetUnassignedRidesAsync();

                foreach (var ride in unassignedRides)
                {
                    await uow.BeginTransactionAsync();
                    try
                    {
                        var latest = await uow.Rides.GetByIdAsync(ride.Id);
                        if (latest == null || (latest.Status != RideStatus.Pending && latest.Status != RideStatus.Searching))
                        {
                            await uow.RollbackAsync();
                            continue;
                        }

                        var nearbyDrivers = await uow.Drivers.GetNearbyDriversAsync(
                            ride.PickupLocation.Latitude,
                            ride.PickupLocation.Longitude,
                            10.0);

                        if (!nearbyDrivers.Any())
                        {
                            if (latest.Status == RideStatus.Pending)
                                latest.MarkSearching();
                            await uow.CommitAsync();
                            continue;
                        }

                        foreach (var driver in nearbyDrivers)
                        {
                            var driverLatest = await uow.Drivers.GetByUserIdAsync(driver.UserId);
                            if (driverLatest == null || !driverLatest.IsAvailable || driverLatest.IsVerified == false)
                                continue;

                            latest.AssignDriver(driverLatest);
                            driverLatest.SetAvailable(false);
                            _logger.LogInformation("Ride {RideId} auto-assigned to driver {DriverId}",
                                ride.Id, driverLatest.Id);
                            break;
                        }

                        await uow.CommitAsync();
                    }
                    catch
                    {
                        await uow.RollbackAsync();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ride matching loop");
            }
            finally
            {
                try { await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken); } catch { }
            }
        }
    }
}
