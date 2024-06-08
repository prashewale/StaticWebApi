using Newtonsoft.Json;

namespace Static.Services.Models;

public interface ITick
{
    public bool Tradable { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal BuyPrice1 { get; set; }
    public int BuyQuantity1 { get; set; }
    public decimal PreviousDayClosePrice { get; set; }
    public Exchange Exchange { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal LastPrice { get; set; }
    public int LastQuantity { get; set; }
    public decimal OpenPrice { get; set; }
    public uint OpenInterest { get; set; }
    public decimal Change { get; set; }
    public decimal PercentageChange { get; set; }
    public uint PreviousOpenInterest { get; set; }
    public decimal SellPrice1 { get; set; }
    public int SellQuantity1 { get; set; }
    public decimal LowerCircuitLimit { get; set; }
    public uint InstrumentToken { get; set; }
    public string InstrumentName { get; set; }
    public uint Volume { get; set; }
    public DateTime TimeStamp { get; set; }
}
