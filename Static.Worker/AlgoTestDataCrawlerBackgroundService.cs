
using iluvadev.ConsoleProgressBar;
using Microsoft.Extensions.Options;
using Static.Services;
using Static.Services.AlgoTest;
using Static.Services.Models;
using Static.Services.Models.AlgoTest;
using Static.Services.Repository;
using System.Linq;
using System.Text.Json;

namespace Static.Worker;

public class AlgoTestDataCrawlerBackgroundService(ILogger<Worker> logger, IAlgoTestApi algoTestApi, IOptions<AlgoTestCredential> algoTestCredentialOptions, IRepoService repoService) : BackgroundService
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
                Instrument? objInstrument = await _repoService.Instruments.GetByName(strInstrumentName);

                objInstrument ??= await _repoService.Instruments.CreateAsync(new Instrument { Exchange = Exchange.NFO, InstrumentName = strInstrumentName });

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

                    objProgressBarTradingDate.Start();

                    foreach (DateTime dtTradingDate in objListOfAllowedTradingDaysInRange)
                    {
                        string strProgressBarTradingDateElement = dtTradingDate.ToString("dd-MM-yyyy");

                        IEnumerable<DateTime> objListOfWorkingHours = GetWorkingHoursForDate(dtTradingDate);

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

                            //for (int i = 0; i < objProgressBarTradingDateTime.Maximum; i++)
                            //{
                            //    string elementName = Guid.NewGuid().ToString();

                            //    Task.Delay(10).Wait(); //Do something
                            //    objProgressBarTradingDateTime.PerformStep(elementName); //Step in ProgressBar. Setting current ElementName
                            //}

                            // Loop through working hours datetime it will have every min datetime from 9:16am to 3:30 pm
                            foreach (DateTime dtTradingDateTime in objListOfWorkingHours)
                            {
                                string elementName = dtTradingDateTime.ToString("dd-MM-yyyy hh:mm tt");

                                // get option chain data from perticular instrument and time of day eg. BANKNIFTY --> 9:30am
                                var objOptionChainForTradingDay = await _algoTestApi.GetOptionChain(strInstrumentName, dtTradingDateTime);

                                // check if value is null or not
                                if (objOptionChainForTradingDay is null) continue;

                                // we got candle in string so we are converting it into datetime for our use.
                                DateTime dtCandleDateTime = objOptionChainForTradingDay.Candle.ConvertToDateTime();

                                // check if Options field is null or not
                                if (objOptionChainForTradingDay.Options is null) continue;

                                // take each expiry date and convert it into list of DateTime 
                                IEnumerable<DateTime> objListOfAlgoTestOptionExpiries = objOptionChainForTradingDay.Options.Keys.Select(x => x.ConvertToDate());

                                // get list of option expiries for instrument id
                                IQueryable<OptionExpiry> objListOfOptionExpiries = await _repoService.OptionExpiries.GetAllAsync(x => x.InstrumentId == objInstrument.Id);

                                IEnumerable<DateTime> objListOfOptionExpiryDates = objListOfOptionExpiries.Select(x => x.ExpiryDate).ToList();

                                // remove already available expiries for perticular instument and convert it into list of option expiry
                                IEnumerable<OptionExpiry> objListOfOptionsExpiryDateToCreate = objListOfAlgoTestOptionExpiries
                                    .ExceptBy(objListOfOptionExpiryDates, x => x)
                                    .Select(x => new OptionExpiry { ExpiryDate = x, InstrumentId = objInstrument.Id });

                                // create all new option expiries in repository
                                await _repoService.OptionExpiries.CreateAllAsync(objListOfOptionsExpiryDateToCreate);

                                //foreach (string strKey in objOptionChainForTradingDay.Options.Keys)
                                //{
                                //    var objOption = objOptionChainForTradingDay.Options[strKey];
                                //    DateTime dtOptionExpiryDate = strKey.ConvertToDate();

                                //    OptionExpiry objOptionExpiryToCreate = new() { ExpiryDate = dtOptionExpiryDate, InstrumentId = objInstrument.Id };
                                //}

                                // loop through each expiry
                                foreach (string strKey in objOptionChainForTradingDay.Options.Keys)
                                {
                                    var objOption = objOptionChainForTradingDay.Options[strKey];
                                    DateTime dtOptionExpiryDate = strKey.ConvertToDate();

                                    // get instance of option expiry from updated repository (it will have id generated for option expiry which will be needed for further operations)
                                    OptionExpiry? objOptionExpiry = objInstrument.ListOfOptionExpiries.FirstOrDefault(x => x.ExpiryDate == dtOptionExpiryDate);
                                    
                                    // ideally it will not process it but have put here for safty only
                                    objOptionExpiry ??= await _repoService.OptionExpiries.CreateAsync(new OptionExpiry { ExpiryDate = dtOptionExpiryDate, InstrumentId = objInstrument.Id });


                                    var objListOfStrikes = await _repoService.Strikes.GetAllAsync(x => x.OptionExpiryId == objOptionExpiry.Id);

                                    //IEnumerable<uint> objListOfStrikePrices = [.. objListOfStrikes.Select(x => x.StrikePrice)];

                                    List<uint> objListOfStrikePricesToCreate = [];
                                    List<int> objListOfStrikeIndexToUpdate = [];

                                    for (int i = 0; i < objOption.Strikes.Count; i++)
                                    {
                                        uint intStrikePrice = objOption.Strikes[i] ?? 0;
                                        if (intStrikePrice == 0) continue;

                                        if (!objListOfStrikes.Any(x => x.StrikePrice == intStrikePrice))
                                        {
                                            objListOfStrikePricesToCreate.Add(intStrikePrice);
                                        }
                                        else
                                        {
                                            objListOfStrikeIndexToUpdate.Add(i);
                                        }
                                    }

                                    if (objListOfStrikePricesToCreate.Count != 0)
                                    {
                                        IEnumerable<Strike> objStrikeToCreate = objListOfStrikePricesToCreate
                                                .Select((x,i) => new Strike { StrikePrice = (uint)x!, OptionExpiryId = objOptionExpiry.Id, ListOfOptionCadles =
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
                                                            TimeStamp = objOption.CallTimestamps[i]?.ConvertToDateTime(),
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
                                                            TimeStamp = objOption.PutTimestamps[i]?.ConvertToDateTime(),
                                                        },

                                                    ]
                                                });

                                        await _repoService.Strikes.CreateAllAsync(objStrikeToCreate);
                                    }
                                    else
                                    {
                                        for (int i = 0; i < objOption.Strikes.Count; i++)
                                        {
                                            if (!objListOfStrikeIndexToUpdate.Contains(i)) continue;

                                            uint intStrikePrice = objOption.Strikes[i] ?? 0;
                                            if (intStrikePrice == 0) continue;

                                            Strike? objStrike = objListOfStrikes.FirstOrDefault(x => x.StrikePrice == intStrikePrice);
                                            
                                            objStrike ??= await _repoService.Strikes.CreateAsync(new Strike { OptionExpiryId = objOptionExpiry.Id, StrikePrice = intStrikePrice });

                                            List<OptionCandle> objListOfOptionCandlesToCreate = [];

                                            string? strCallTimeStamp = objOption.CallTimestamps[i];
                                            if (!string.IsNullOrWhiteSpace(strCallTimeStamp))
                                            {
                                                DateTime dtCandleTimeStamp = strCallTimeStamp.ConvertToDateTime();
                                                OptionCandle? objOptionCandleCall = objStrike.ListOfOptionCadles.FirstOrDefault(x => x.TimeStamp == dtCandleDateTime && x.OptionType == OptionType.Call);
                                                if (objOptionCandleCall is null)
                                                {
                                                    OptionCandle objCandleToCreate = new()
                                                    {
                                                        ClosePrice = objOption.CallClosePrices[i],
                                                        Delta = objOption.CallDeltas[i],
                                                        Gamma = objOption.CallGammas[i],
                                                        ImpliedFuture = objOption.CallImpliedFutures[i],
                                                        ImpliedValume = objOption.CallImpliedVolumes[i],
                                                        StrikeId = objStrike.Id,
                                                        OptionType = OptionType.Call,
                                                        Rho = objOption.CallRhos[i],
                                                        Theta = objOption.CallDeltas[i],
                                                        TimeStamp = dtCandleTimeStamp,
                                                    };
                                                    objListOfOptionCandlesToCreate.Add(objCandleToCreate);
                                                }

                                                //objOptionCandleCall ??= await _repoService.OptionCadles.CreateAsync(objCandleToCreate);
                                            }


                                            string? strPutTimeStamp = objOption.PutTimestamps[i];
                                            if (!string.IsNullOrWhiteSpace(strPutTimeStamp))
                                            {
                                                DateTime dtCandleTimeStamp = strPutTimeStamp.ConvertToDateTime();
                                                OptionCandle? objOptionCandlePut = objStrike.ListOfOptionCadles.FirstOrDefault(x => x.TimeStamp == dtCandleDateTime && x.OptionType == OptionType.Put);
                                                if (objOptionCandlePut is null)
                                                {
                                                    OptionCandle objCandleToCreate = new()
                                                    {
                                                        ClosePrice = objOption.PutClosePrices[i],
                                                        Delta = objOption.PutDeltas[i],
                                                        Gamma = objOption.PutGammas[i],
                                                        ImpliedFuture = objOption.PutImpliedFutures[i],
                                                        ImpliedValume = objOption.PutImpliedVolumes[i],
                                                        StrikeId = objStrike.Id,
                                                        OptionType = OptionType.Call,
                                                        Rho = objOption.PutRhos[i],
                                                        Theta = objOption.PutDeltas[i],
                                                        TimeStamp = dtCandleTimeStamp,
                                                    };
                                                    objListOfOptionCandlesToCreate.Add(objCandleToCreate);
                                                }

                                                //objOptionCandlePut ??= await _repoService.OptionCadles.CreateAsync(objCandleToCreate);
                                            }

                                            if(objListOfOptionCandlesToCreate.Count != 0)
                                            {
                                                await _repoService.OptionCadles.CreateAllAsync(objListOfOptionCandlesToCreate);
                                            }

                                        }
                                    }
                                }
                                objProgressBarTradingDateTime.PerformStep(elementName); //Step in ProgressBar. Setting current ElementName
                                objProgressBarTradingDate.PerformStep(strProgressBarTradingDateElement); //Step in ProgressBar. Setting current ElementName

                            }

                        }
                    }
                }

                
            }

            //var dayContracts = await _algoTestApi.GetDayContract(AlgoTestApiConstant.INDICES_BANKNIFTY, DateTime.Now.AddMonths(-10));

            //var optionChain = await _algoTestApi.GetOptionChain(AlgoTestApiConstant.INDICES_BANKNIFTY, new DateTime(2024, 5, 17, 10, 15, 0));

            _logger.LogWarning("--> Option Chain Process Ended : {time}", DateTimeOffset.Now);
        }
        //while (!stoppingToken.IsCancellationRequested)
        //{

        //    //await Task.Delay(5000, stoppingToken);
        //}
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
