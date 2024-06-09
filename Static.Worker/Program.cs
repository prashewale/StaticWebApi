using Static.Services;
using Static.Worker;
using Static.Worker.Infrastructure;
using System.Threading;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSingleton<HostedServiceManager>();
builder.Services.AddAlgoTestClient(builder.Configuration);
builder.Services.AddDataServices(builder.Configuration, builder.Services.BuildServiceProvider());

//builder.Services.AddHostedService<AlgoTestDataCrawlerBackgroundService>();

//builder.Services.AddHostedService<Worker>();

var host = builder.Build();
var serviceManager = host.Services.GetRequiredService<HostedServiceManager>();
List<Task> tasks = [];
for (int i = 0; i < 1; i++)
{
    var cancellationTokenSource = new CancellationTokenSource();
    Task t = Task.Run(async () => await serviceManager.StartNewServiceAsync(i, cancellationTokenSource.Token), cancellationTokenSource.Token);
    tasks.Add(t);
}

Task.WhenAll(tasks).Wait(); 

host.Run();
