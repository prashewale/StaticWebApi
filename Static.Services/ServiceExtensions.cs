using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using Static.Services.AlgoTest;
using Static.Services.FlatTrade;
using Static.Services.Models;
using Static.Services.Models.AlgoTest;
using Static.Services.Models.FlatTrade;
using Static.Services.Repository;
using Static.Services.Repository.DbContexts;
using Static.Services.WebSocketClientServices;
using System;

namespace Static.Services;

public static class ServiceExtensions
{
    public static IServiceCollection AddFlatTradeClient(this IServiceCollection services, IConfiguration configuration)
    {
        var flatTradeSection = configuration.GetSection(FlatTradeApiConstant.FLATTRADE_CREDENTIAL_PROPERTY_NAME);
        services.Configure<FlatTradeCredential>(flatTradeSection);

        services.AddTransient<IWebSocketClientService,  WebSocketClientService>();
        services.AddTransient(typeof(TickerService<>));

        services.AddTransient<IFlatTradeApi, RefitFlatTradeApiService>();
        services.AddTransient<AdditionalHeadersHandler>();

        services.AddRefitClient<IFlatTradeApiReFit>(new RefitSettings(new NewtonsoftJsonContentSerializer()))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
            .ConfigureHttpClient(httpClient =>
            {
                FlatTradeCredential? credential = flatTradeSection.Get<FlatTradeCredential>();
                if (credential == null)
                    throw new ArgumentNullException(nameof(credential));

                if (string.IsNullOrEmpty(credential.ApiUrl))
                    throw new ArgumentException(nameof(credential.ApiUrl));

                httpClient.BaseAddress = new Uri(credential.ApiUrl);
                httpClient.DefaultRequestHeaders.Add("User-Agent", FlatTradeApiConstant.USER_AGENT);
                
            })
            .AddHttpMessageHandler<AdditionalHeadersHandler>();

        return services;
    }

    public static IServiceCollection AddAlgoTestClient(this IServiceCollection services, IConfiguration configuration)
    {

        var algoTestSection = configuration.GetSection(AlgoTestApiConstant.ALGOTEST_CREDENTIAL_PROPERTY_NAME);

        services.Configure<AlgoTestCredential>(algoTestSection);

        services.AddTransient<IAlgoTestApi, RefitAlgoTestApiService>(); 
        services.AddTransient<RequestUrlHandler>();

        services.AddRefitClient<IAlgoTestApiRefit>(new RefitSettings(new NewtonsoftJsonContentSerializer()))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler())
            .ConfigureHttpClient(httpClient =>
            {
                AlgoTestCredential? credential = algoTestSection.Get<AlgoTestCredential>();
                if (credential == null)
                    throw new ArgumentNullException(nameof(credential));

                if (string.IsNullOrEmpty(credential.BaseHost))
                    throw new ArgumentException(nameof(credential.BaseHost));

                httpClient.BaseAddress = new Uri($"https://{credential.BaseHost}");
                httpClient.DefaultRequestHeaders.Add("User-Agent", AlgoTestApiConstant.USER_AGENT);

            })
            .AddHttpMessageHandler<RequestUrlHandler>();
        
        return services;
    }

    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        //services.AddDbContext<ApplicationDbContext>(opt =>
        //{
        //    string? connectionString = configuration.GetConnectionString(RepositoryConstant.APPLICATION_DB_CONNECTION_PROPERTY_NAME);
        //    if (string.IsNullOrEmpty(connectionString))
        //    {
        //        throw new Exception($"Connection string ${RepositoryConstant.APPLICATION_DB_CONNECTION_PROPERTY_NAME} is not provided.");
        //    }
        //    opt.UseSqlServer(connectionString);
        //});

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        string connectionString = RepositoryConstant.APPLICATION_DB_CONNECTION_STRING;

        optionsBuilder.UseSqlServer(connectionString);

        services.AddSingleton(db => new ApplicationDbContext(optionsBuilder.Options));

        //using var scope = serviceProvider.CreateScope();
        //var scopedServiceProvider = scope.ServiceProvider;
        //var scopedServices = scopedServiceProvider.GetService<ApplicationDbContext>();
        //if (scopedServices == null) throw new ArgumentNullException($"{nameof(scopedServices)}");

        services.AddTransient(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddTransient<IRepoService, RepoService>();
        services.AddTransient<IInstrumentRepo, InstrumentRepo>();

        services.AddSingleton<IRepository<ApiKeyModel>, MockRepository>();

        return services;
    }
}
