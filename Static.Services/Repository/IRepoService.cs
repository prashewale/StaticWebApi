using Static.Services.Models;

namespace Static.Services.Repository;

public interface IRepoService
{
    public IInstrumentRepo Instruments { get; set; }
    public IRepository<OptionCandle> OptionCadles { get; set; }
    public IRepository<OptionExpiry> OptionExpiries { get; set; }
    public IRepository<Strike> Strikes { get; set; }
}
