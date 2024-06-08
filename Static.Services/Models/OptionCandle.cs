namespace Static.Services.Models;

public class OptionCandle : FullAuditModel
{
    public CandleDuration Duration { get; set; } = CandleDuration.OneMinute;
    public int StrikeId { get; set; }
    public virtual Strike MyStrike { get; set; }
    public OptionType OptionType { get; set; }
    public decimal? ClosePrice { get; set; }
    public decimal? Delta { get; set; }
    public decimal? Gamma { get; set; }
    public decimal? ImpliedFuture { get; set; }
    public decimal? ImpliedValume { get; set; }
    public decimal? Rho { get; set; }
    public decimal? Theta { get; set; }
    public DateTime? TimeStamp { get; set; }

}


public enum CandleDuration
{
    OneMinute = 1,
    ThreeMinute,
    FiveMinute,
    FifteenMinute,
    HalfMinute,
    OneHour
}

public enum OptionType
{
    Call = 1,
    Put
}