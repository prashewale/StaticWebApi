using Static.Services.Models;

namespace Static.Services.Repository;

public interface IInstrumentRepo: IRepository<Instrument>
{
    Task<Instrument?> GetByName(string name);
}
