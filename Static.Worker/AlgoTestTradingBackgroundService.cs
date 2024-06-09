
using iluvadev.ConsoleProgressBar;
using Microsoft.Extensions.Options;
using Static.Services;
using Static.Services.AlgoTest;
using Static.Services.Models;
using Static.Services.Models.AlgoTest;
using Static.Services.Repository;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Static.Worker;

public class AlgoTestTradingBackgroundService(ILogger<Worker> logger, IAlgoTestApi algoTestApi, IOptions<AlgoTestCredential> algoTestCredentialOptions, IRepoService repoService) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    //private readonly IFlatTradeApi _flateTradeApi = flateTradeApi;
    //private readonly TickerService<FlatTradeTick> _tickerService = tickerService;
    private readonly IAlgoTestApi _algoTestApi = algoTestApi;
    private readonly IRepoService _repoService = repoService;
    private readonly AlgoTestCredential _algoTestCredential = algoTestCredentialOptions.Value;

    private readonly List<string> _listOfInstrumentNames = [AlgoTestApiConstant.INDICES_BANKNIFTY];

    public DateTime StartDate { get; set; } = DateTime.Now.Date.AddDays(-1);
    public DateTime EndDate { get; set; } = DateTime.Now.Date.AddDays(-10);
    public int InstanceNumber { get; set; }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_logger.IsEnabled(LogLevel.Warning) )
        {
            _logger.LogWarning("--> Option Chain Process Started : {time}", DateTimeOffset.Now);

            AlgoTestLoginRequest algoTestLoginRequest = new()
            {
                AnonymousId = Guid.NewGuid().ToString(),
                PhoneNumber = _algoTestCredential.PhoneNumber,
                Password = _algoTestCredential.Password,
            };

            var objLoginResponse = await _algoTestApi.Login(algoTestLoginRequest);

            var objTradingCalender = await _algoTestApi.GetTradingCalender();

            ArgumentNullException.ThrowIfNull(objTradingCalender);

            IEnumerable<DateTime> objListOfTradingDays = objTradingCalender.TradingDays.ConvertListToDate();

            IEnumerable<DateTime> objListOfAllowedTradingDaysInRange = [.. objListOfTradingDays
                        .Where(x => x <= StartDate && x >= EndDate)
                        .OrderByDescending(x => x)];

            foreach (string strInstrumentName in _listOfInstrumentNames)
            {
                //Create the ProgressBar
                using (var objProgressBarTradingDate = new ProgressBar(initialPosition: InstanceNumber * 10 + 4,  autoStart: true) { Maximum = objListOfAllowedTradingDaysInRange.SelectMany(x => GetWorkingHoursForDate(x)).Count() })
                {
                    // Hide Text
                    objProgressBarTradingDate.Text.Body.SetVisible(false);

                    // Clear "Description Text"
                    objProgressBarTradingDate.Text.Description.Clear();

                    // Hide "Margins"
                    objProgressBarTradingDate.Layout.Margins.SetVisible(false);

                    // Hide "Marquee"
                    objProgressBarTradingDate.Layout.Marquee.SetVisible(false);

                    // Setting Body Colors
                    objProgressBarTradingDate.Layout.Body.Pending.SetForegroundColor(ConsoleColor.White)
                                          .SetBackgroundColor(ConsoleColor.DarkRed);
                    objProgressBarTradingDate.Layout.Body.Progress.SetForegroundColor(ConsoleColor.Black)
                                           .SetBackgroundColor(ConsoleColor.DarkGreen);

                    // Setting Body Text (internal text), from Layout
                    objProgressBarTradingDate.Layout.Body.Text.SetVisible(true).SetValue(pb =>
                    {
                        if (pb.IsDone)
                            return $"{pb.Value} elements processed in {pb.TimeProcessing.ToProcessTimeString()}.";
                        else
                            return $"{pb.Percentage}%... Remaining: {pb.TimeRemaining?.ToProcessTimeString()}. - Current: {pb.ElementName}";
                    });

                    // Setting ProgressBar width
                    objProgressBarTradingDate.Layout.ProgressBarWidth = Console.BufferWidth;

                    //objProgressBarTradingDate.Start();

                    Dictionary<DateTime, List<OptionExpiry>> objDictOfTradingDays = [];

                    foreach (DateTime dtTradingDate in objListOfAllowedTradingDaysInRange)
                    {
                        string strProgressBarTradingDateElement = dtTradingDate.ToString("dd-MM-yyyy");

                        IEnumerable<DateTime> objListOfWorkingHours = GetWorkingHoursForDate(dtTradingDate);


                        List<OptionExpiry> objListOfOptionExpiries = [];

                        //Create the ProgressBar
                        using (var objProgressBarTradingDateTime = new ProgressBar(InstanceNumber * 10 + 5, autoStart:true) { Maximum = objListOfWorkingHours.Count(),  })
                        {
                            //Clear "Description Text"
                            objProgressBarTradingDateTime.Text.Description.Clear();

                            //Setting "Description Text" when "Processing"
                            objProgressBarTradingDateTime.Text.Description.Processing.AddNew().SetValue(pb => $"Element: {pb.ElementName}");
                            objProgressBarTradingDateTime.Text.Description.Processing.AddNew().SetValue(pb => $"Count: {pb.Value}");
                            objProgressBarTradingDateTime.Text.Description.Processing.AddNew().SetValue(pb => $"Processing time: {pb.TimeProcessing.ToProcessTimeString()}.");
                            objProgressBarTradingDateTime.Text.Description.Processing.AddNew().SetValue(pb => $"Estimated remaining time: {pb.TimeRemaining?.ToProcessTimeString()}.");

                            //Setting "Description Text" when "Done"
                            objProgressBarTradingDateTime.Text.Description.Done.AddNew().SetValue(pb => $"{pb.Value} elements in {pb.TimeProcessing.TotalSeconds}s.");


                            // Loop through working hours datetime it will have every min datetime from 9:16am to 3:30 pm
                            foreach (DateTime dtTradingDateTime in objListOfWorkingHours)
                            {
                                string elementName = dtTradingDateTime.ToString("dd-MM-yyyy hh:mm tt");

                                // get option chain data from perticular instrument and time of day eg. BANKNIFTY --> 9:30am
                                var objOptionChainForTradingDay = await _algoTestApi.GetOptionChain(strInstrumentName, dtTradingDateTime);

                                // check if value is null or not
                                if (objOptionChainForTradingDay is null) continue;

                                // check if value is null or not
                                if (objOptionChainForTradingDay.Options is null) continue;

                                foreach (string strExpiryDateString in objOptionChainForTradingDay.Options.Keys)
                                {
                                    if (objOptionChainForTradingDay.Options is null) continue;

                                    var objOption = objOptionChainForTradingDay.Options[strExpiryDateString];
                                    DateTime dtOptionExpiryDate = strExpiryDateString.ConvertToDate();

                                    List<Strike> objListOfStrikes = objOption.Strikes
                                               .Select((x, i) => new Strike
                                               {
                                                   StrikePrice = (uint)x!,
                                                   ListOfOptionCadles =
                                                   [
                                                       new OptionCandle
                                                        {
                                                            ClosePrice = objOption.CallClosePrices[i],
                                                            Delta = objOption.CallDeltas[i],
                                                            Gamma = objOption.CallGammas[i],
                                                            ImpliedFuture = objOption.CallImpliedFutures[i],
                                                            ImpliedValume = objOption.CallImpliedVolumes[i],
                                                            OptionType = OptionType.Call,
                                                            Rho = objOption.CallRhos[i],
                                                            Theta = objOption.CallDeltas[i],
                                                            TimeStamp = dtTradingDateTime,
                                                        },
                                                        new OptionCandle
                                                        {
                                                            ClosePrice = objOption.PutClosePrices[i],
                                                            Delta = objOption.PutDeltas[i],
                                                            Gamma = objOption.PutGammas[i],
                                                            ImpliedFuture = objOption.PutImpliedFutures[i],
                                                            ImpliedValume = objOption.PutImpliedVolumes[i],
                                                            OptionType = OptionType.Put,
                                                            Rho = objOption.PutRhos[i],
                                                            Theta = objOption.PutDeltas[i],
                                                            TimeStamp = dtTradingDateTime,
                                                        },

                                                   ]
                                               })
                                               .ToList();


                                    OptionExpiry objOptionExpiry = new() { ExpiryDate = dtOptionExpiryDate, ListOfStrikes = objListOfStrikes };
                                    objListOfOptionExpiries.Add(objOptionExpiry);
                                }
                                
                                objProgressBarTradingDateTime.PerformStep(elementName); //Step in ProgressBar. Setting current ElementName

                            }
                        }

                        objDictOfTradingDays.Add(dtTradingDate, objListOfOptionExpiries);

                        objProgressBarTradingDate.PerformStep(strProgressBarTradingDateElement); //Step in ProgressBar. Setting current ElementName

                    }
                }

                
            }

            _logger.LogWarning("--> Option Chain Process Ended : {time}", DateTimeOffset.Now);
        }
       
    }

    private static IEnumerable<DateTime> GetWorkingHoursForDate(DateTime tradingDate)
    {
        List<DateTime> workingHours = [];

        DateTime dtStartDateTime = tradingDate.Date.AddHours(9).AddMinutes(16);
        DateTime dtEndDateTime = tradingDate.Date.AddHours(15).AddMinutes(30);

        DateTime dtInitialDateTime = dtEndDateTime;
        while (dtInitialDateTime >= dtStartDateTime)
        {
            workingHours.Add(dtInitialDateTime);
            dtInitialDateTime = dtInitialDateTime.AddMinutes(-1);
        }

        return workingHours;
    }

    private static void OnConnect()
    {
        Console.WriteLine("Connected ticker");
    }

    private static void OnClose()
    {
        Console.WriteLine("Closed ticker");
    }

    private static void OnError(string Message)
    {
        Console.WriteLine("Error: " + Message);
    }

    private static void OnNoReconnect()
    {
        Console.WriteLine("Not reconnecting");
    }

    private static void OnReconnect()
    {
        Console.WriteLine("Reconnecting");
    }

    private static void OnTick(ITick TickData)
    {
        Console.WriteLine("Tick " + (JsonSerializer.Serialize(TickData) ?? ""));
    }
}
