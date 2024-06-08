using Newtonsoft.Json;

namespace Static.Services.Models.FlatTrade;

public class FlatTradeQuickAuthResponse
{
    [JsonProperty(FlatTradeApiConstant.PROPERTY_REQUEST_TIME)]
    public string RequestTime { get; set; }

    [JsonProperty(FlatTradeApiConstant.PROPERTY_UID)]
    public string UserId { get; set; }

    [JsonProperty(FlatTradeApiConstant.PROPERTY_ACCESS_TYPE)]
    public List<string> AccessTypes { get; set; } = [];

    [JsonProperty(FlatTradeApiConstant.PROPERTY_FULL_NAME)]
    public string UserFullName { get; set; }

    [JsonProperty(FlatTradeApiConstant.PROPERTY_TOKEN)]
    public string Token { get; set; }


}
