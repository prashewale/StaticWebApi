using Static.Services.Models;

namespace Static.Services.Repository;

public class RepoService(IInstrumentRepo instruments, IRepository<OptionCandle> optionCandles, IRepository<OptionExpiry> optionExpiries, IRepository<Strike> strikes) : IRepoService
{
    public IInstrumentRepo Instruments { get; set; } = instruments;
    public IRepository<OptionCandle> OptionCadles { get; set; } = optionCandles;
    public IRepository<OptionExpiry> OptionExpiries { get; set; } = optionExpiries;
    public IRepository<Strike> Strikes { get; set; } = strikes;
}
