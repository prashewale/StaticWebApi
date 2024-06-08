using Refit;
using Static.Services.Models.AlgoTest;

namespace Static.Services.AlgoTest;

public interface IAlgoTestApi
{
    Task<AlgoTestLoginResponse?> Login(AlgoTestLoginRequest loginRequest);
    Task<AlgoTestMarketHolidayResponse?> IsMarketHoliday(DateTime date);
    Task<AlgoTestTradingCalenderResponse?> GetTradingCalender();
    Task<AlgoTestDayContractResponse?> GetDayContract(string underlying, DateTime date);
    Task<AlgoTestOptionChainResponse?> GetOptionChain(string underlying, DateTime candle);
}
