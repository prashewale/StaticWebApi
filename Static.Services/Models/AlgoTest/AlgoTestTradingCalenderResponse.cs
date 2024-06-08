using Newtonsoft.Json;

namespace Static.Services.Models.AlgoTest;

public class AlgoTestTradingCalenderResponse
{
    [JsonProperty("trading_days")]
    public List<string> TradingDays { get; set; }

    [JsonProperty("data_ranges")]
    public AlgoTestDateRange DataRanges { get; set; }
}

public class AlgoTestDateRange
{
    [JsonProperty("BANKEX")]
    public List<string> Bankex { get; set; }

    [JsonProperty("BANKNIFTY")]
    public List<string> BankNifty { get; set; }

    [JsonProperty("FINNIFTY")]
    public List<string> FinNifty { get; set; }

    [JsonProperty("MIDCPNIFTY")]
    public List<string> MIDCNifty { get; set; }

    [JsonProperty("NIFTY")]
    public List<string> Nifty { get; set; }

    [JsonProperty("SENSEX")]
    public List<string> Sensex { get; set; }

}
