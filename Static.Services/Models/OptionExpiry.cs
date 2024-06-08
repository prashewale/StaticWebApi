namespace Static.Services.Models;

public class OptionExpiry : FullAuditModel
{
    public DateTime ExpiryDate { get; set; }
    public int InstrumentId { get; set; }
    public virtual Instrument MyInstrument { get; set; }

    public virtual ICollection<Strike> ListOfStrikes { get; set; } = [];
}
