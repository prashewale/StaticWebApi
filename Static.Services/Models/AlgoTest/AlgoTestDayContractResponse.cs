using Newtonsoft.Json;

namespace Static.Services.Models.AlgoTest;

public class AlgoTestDayContractResponse
{
    [JsonProperty("date")]
    public string Date { get; set; }

    [JsonProperty("strikes")]
    public Dictionary<string, List<uint>> Strikes { get; set; }

    [JsonProperty("underlying")]
    public string Underlying { get; set; }
}
