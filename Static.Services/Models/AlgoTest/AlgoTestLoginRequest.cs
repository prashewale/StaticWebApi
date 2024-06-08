using Newtonsoft.Json;
using Refit;

namespace Static.Services.Models.AlgoTest;

public class AlgoTestLoginRequest
{
    [JsonProperty("anonymousId")]
    public string AnonymousId { get; set; }

    [JsonProperty("phoneNumber")]
    public string PhoneNumber { get; set; }

    [JsonProperty("password")]
    public string Password { get; set; }
}
