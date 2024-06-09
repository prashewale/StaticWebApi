using Microsoft.Extensions.Options;
using Refit;
using Static.Services;
using Static.Services.AlgoTest;
using Static.Services.FlatTrade;
using Static.Services.Models;
using Static.Services.Models.AlgoTest;
using Static.Services.Models.FlatTrade;
using Static.Services.Repository;
using Static.Worker.Infrastructure;
using System.Text.Json;
using System.Threading;

namespace Static.Worker
{
    public class Worker(ILogger<Worker> logger, HostedServiceManager serviceManager, IAlgoTestApi algoTestApi, IOptions<AlgoTestCredential> algoTestCredentialOptions) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly HostedServiceManager _serviceManager = serviceManager;
        private readonly int _maxThreadCount = 3;
        private readonly IAlgoTestApi _algoTestApi = algoTestApi;
        private readonly AlgoTestCredential _algoTestCredential = algoTestCredentialOptions.Value;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
           
            int intThreadNumber = 0; 

            while (!stoppingToken.IsCancellationRequested)
            {
                if (intThreadNumber <= 0)
                {
                    await serviceManager.StartNewServiceAsync(intThreadNumber, stoppingToken);

                    intThreadNumber++;
                }
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
