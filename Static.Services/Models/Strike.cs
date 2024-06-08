namespace Static.Services.Models;

public class Strike : FullAuditModel
{
    public uint StrikePrice { get; set; }
    public int OptionExpiryId { get; set; }
    public virtual OptionExpiry OptionExpiry { get; set; }

    public virtual ICollection<OptionCandle> ListOfOptionCadles { get; set; } = [];
}
