using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Static.Services.Models;
using Static.Services.Repository.DbContexts;

namespace Static.Services.Repository;

public class InstrumentRepo: GenericRepository<Instrument>, IInstrumentRepo
{
    private readonly ApplicationDbContext _dbContext;

    public InstrumentRepo(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Instrument?> GetByName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return await _dbContext.Instruments.FirstOrDefaultAsync(x => x.InstrumentName == name);
    }

    //~InstrumentRepo()
    //{
    //    _dbContext?.Dispose();
    //}
}
