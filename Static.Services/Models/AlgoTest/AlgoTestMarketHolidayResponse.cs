using Newtonsoft.Json;

namespace Static.Services.Models.AlgoTest;

public class AlgoTestMarketHolidayResponse
{
    [JsonProperty("date")]
    public string Date { get; set; }

    [JsonProperty("is_holiday")]
    public bool IsHoliday { get; set; }
}
