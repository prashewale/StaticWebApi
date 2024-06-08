using Static.Services.Models.FlatTrade;

namespace Static.Services.FlatTrade;

public interface IFlatTradeApi
{
    Task<FlatTradeQuickAuthResponse?> Login();

    Task<FlatTradeUserDetailResponse?> GetUserDetails();
    public List<string> Cookies { get; }
    public string Token { get; }
}
