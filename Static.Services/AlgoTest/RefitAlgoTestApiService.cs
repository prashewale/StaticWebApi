using Static.Services.Models.AlgoTest;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Static.Services.AlgoTest;

public class RefitAlgoTestApiService(IAlgoTestApiRefit algoTestApiRefit) : IAlgoTestApi
{
    private readonly IAlgoTestApiRefit _algoTestApiRefit = algoTestApiRefit;

    public async Task<AlgoTestLoginResponse?> Login(AlgoTestLoginRequest loginRequest)
    {
        var response = await _algoTestApiRefit.Login(loginRequest);

        return response.IsSuccessStatusCode ? response.Content : null;
    }

    public async Task<AlgoTestMarketHolidayResponse?> IsMarketHoliday(DateTime date)
    {
        string dateString = ConvertToRequiredDateFormat(date);
        var response = await _algoTestApiRefit.IsMarketHoliday(dateString);

        return response.IsSuccessStatusCode ? response.Content : null;
    }

    public async Task<AlgoTestTradingCalenderResponse?> GetTradingCalender()
    {
        var response = await _algoTestApiRefit.GetTradingCalender();

        return response.IsSuccessStatusCode ? response.Content : null;
    }

    public async Task<AlgoTestDayContractResponse?> GetDayContract(string underlying, DateTime date)
    {
        string dateString = ConvertToRequiredDateFormat(date);
        var response = await _algoTestApiRefit.GetDayContract(underlying, dateString);

        return response.IsSuccessStatusCode ? response.Content : null;
    }

    public async Task<AlgoTestOptionChainResponse?> GetOptionChain(string underlying, DateTime candle)
    {
        string dateString = ConvertToRequiredDateTimeFormat(candle);
        var response = await _algoTestApiRefit.GetOptionChain(underlying, dateString);

        return response.IsSuccessStatusCode ? response.Content : null;
    }



    private static string ConvertToRequiredDateTimeFormat(DateTime date)
    {
        return date.ToString("yyyy-MM-ddTHH:mm:ss");
    }
    private static string ConvertToRequiredDateFormat(DateTime date)
    {
        return date.ToString("yyyy-MM-dd");
    }
}
