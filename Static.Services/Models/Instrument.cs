namespace Static.Services.Models;

public class Instrument : FullAuditModel
{
    public string InstrumentName { get; set; }
    public Exchange Exchange { get; set; }

    public virtual ICollection<OptionExpiry> ListOfOptionExpiries { get; set; } = [];

}
