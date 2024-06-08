using Newtonsoft.Json;

namespace Static.Services.Models.FlatTrade;

public class FlatTradeTick : ITick
{
    [JsonProperty("ap")]
    public decimal AveragePrice { get; set; }

    [JsonProperty("bp1")]
    public decimal BuyPrice1 { get; set; }

    [JsonProperty("bq1")]
    public int BuyQuantity1 { get; set; }

    [JsonProperty("c")]
    public decimal PreviousDayClosePrice { get; set; }

    [JsonProperty("e")]
    public Exchange Exchange { get; set; }

    [JsonProperty("h")]
    public decimal High { get; set; }

    [JsonProperty("l")]
    public decimal Low { get; set; }

    [JsonProperty("lp")]
    public decimal LastPrice { get; set; }

    [JsonProperty("ls")]
    public int LastQuantity { get; set; }

    [JsonProperty("o")]
    public decimal OpenPrice { get; set; }

    [JsonProperty("oi")]
    public uint OpenInterest { get; set; }

    [JsonProperty("pc")]
    public decimal PercentageChange { get; set; }

    [JsonProperty("poi")]
    public uint PreviousOpenInterest { get; set; }

    [JsonProperty("sp1")]
    public decimal SellPrice1 { get; set; }

    [JsonProperty("sq1")]
    public int SellQuantity1 { get; set; }

    [JsonProperty("ti")]
    public decimal LowerCircuitLimit { get; set; }

    [JsonProperty("tk")]
    public uint InstrumentToken { get; set; }

    [JsonProperty("ts")]
    public string InstrumentName { get; set; }

    [JsonProperty("v")]
    public uint Volume { get; set; }
    public bool Tradable { get; set; }
    public decimal Change { get; set; }
    public DateTime TimeStamp { get; set; }
}
