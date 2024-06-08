using Newtonsoft.Json;

namespace Static.Services.Models.AlgoTest;

public class AlgoTestLoginResponse
{
    public string Role { get; set; }
    public string Username { get; set; }

    [JsonProperty("_id")]
    public string Id { get; set; }
}
