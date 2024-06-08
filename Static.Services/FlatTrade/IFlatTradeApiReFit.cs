using Refit;
using Static.Services.Models.FlatTrade;

namespace Static.Services.FlatTrade;

public interface IFlatTradeApiReFit
{
    [Post("/QuickAuth")]
    Task<ApiResponse<FlatTradeQuickAuthResponse>> Login([Body] string data); 
    
    [Post("/UserDetails")]
    Task<ApiResponse<FlatTradeUserDetailResponse>> GetUserDetails([Body] string data);

}
