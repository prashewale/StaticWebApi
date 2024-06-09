using Microsoft.Extensions.Options;
using Static.Services.AlgoTest;
using Static.Services.Models.AlgoTest;
using Static.Services.Repository;

namespace Static.Worker.Infrastructure;

public class HostedServiceManager(IServiceProvider serviceProvider, ILogger<HostedServiceManager> logger )
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<HostedServiceManager> _logger = logger;
    private readonly List<BackgroundService> _runningServices = [];
    public async Task StartNewServiceAsync(int intThreadNumber, CancellationToken cancellationToken = default)
    {
        var hostedService = ActivatorUtilities.CreateInstance<AlgoTestTradingBackgroundService>(_serviceProvider);
        hostedService.StartDate = DateTime.Now.Date.AddDays(-1 * intThreadNumber - 8);
        hostedService.EndDate = hostedService.StartDate;
        hostedService.InstanceNumber = intThreadNumber;

        var cancellationTokenSource = new CancellationTokenSource();
        await Task.Run(async() => await hostedService.StartAsync(cancellationTokenSource.Token), cancellationToken);
        _logger.LogWarning($"Started service instance {intThreadNumber}");
    }

    public async Task StopAllServicesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var service in _runningServices)
        {
            await service.StopAsync(CancellationToken.None);
        }
        _runningServices.Clear();
        _logger.LogInformation("Stopped all service instances");
    }
}