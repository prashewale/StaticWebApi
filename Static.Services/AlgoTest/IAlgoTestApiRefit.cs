using Refit;
using Static.Services.Models.AlgoTest;

namespace Static.Services.AlgoTest;

public interface IAlgoTestApiRefit
{
    [Post("/api/login")]
    Task<ApiResponse<AlgoTestLoginResponse>> Login([Body] AlgoTestLoginRequest loginRequest);

    [Get("/api/calendar/is-market-holiday/{date}")]
    Task<ApiResponse<AlgoTestMarketHolidayResponse>> IsMarketHoliday(string date);

    [Get("/trading-calendar")]
    Task<ApiResponse<AlgoTestTradingCalenderResponse>> GetTradingCalender();

    [Get("/day-contracts")]
    Task<ApiResponse<AlgoTestDayContractResponse>> GetDayContract(string underlying, string date);

    [Get("/option-chain")]
    Task<ApiResponse<AlgoTestOptionChainResponse>> GetOptionChain(string underlying, string candle);
}
