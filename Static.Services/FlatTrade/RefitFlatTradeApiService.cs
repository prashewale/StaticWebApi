using Microsoft.Extensions.Options;
using Static.Services.Models.FlatTrade;

namespace Static.Services.FlatTrade;

public class RefitFlatTradeApiService(IFlatTradeApiReFit apiRefit, IOptions<FlatTradeCredential> credentialOptions) : IFlatTradeApi
{
    private readonly IFlatTradeApiReFit _apiRefit = apiRefit;
    private readonly FlatTradeCredential _credential = credentialOptions.Value;
    public List<string> Cookies { get; private set; } = new List<string>();
    public string Token { get; private set; } = string.Empty;

    public async Task<FlatTradeUserDetailResponse?> GetUserDetails()
    {
        if(string.IsNullOrEmpty(Token)) return null;

        Dictionary<string, string> userDetailsBodyParams = new()
        {
            [FlatTradeApiConstant.REQUEST_USER_ID] = _credential.UserId,
            [FlatTradeApiConstant.REQUEST_TOKEN] = Token,
        };

        string userDetailsBody = FlatTradeApiConstant.USER_DETAILS_REQUEST_BODY_TEMPLATE.Replace(userDetailsBodyParams);

        var userDetailsResponse = await _apiRefit.GetUserDetails(userDetailsBody);

        if (!userDetailsResponse.IsSuccessStatusCode)
        {
            return null;
        }

        return userDetailsResponse.Content;
    }

    public async Task<FlatTradeQuickAuthResponse?> Login()
    {
        Dictionary<string, string> loginBodyParams = new()
        {
            [FlatTradeApiConstant.REQUEST_USER_ID] = _credential.UserId,
            [FlatTradeApiConstant.QUICK_AUTH_REQUEST_PASSWORD] = _credential.Password,
            [FlatTradeApiConstant.QUICK_AUTH_REQUEST_FACTOR2] = _credential.PanNumber,
            [FlatTradeApiConstant.QUICK_AUTH_REQUEST_APK_VERSION] = _credential.ApkVersion,
            [FlatTradeApiConstant.QUICK_AUTH_REQUEST_IMEI] = Guid.NewGuid().ToString(),
            [FlatTradeApiConstant.QUICK_AUTH_REQUEST_VC] = _credential.VC,
            [FlatTradeApiConstant.QUICK_AUTH_REQUEST_SOURCE] = _credential.Source,
            [FlatTradeApiConstant.QUICK_AUTH_REQUEST_APP_KEY] = _credential.AppKey,
            [FlatTradeApiConstant.QUICK_AUTH_REQUEST_ADDL_DIV_INF] = _credential.UserAgent,
        };

        string loginBody = FlatTradeApiConstant.QUICK_AUTH_REQUEST_BODY_TEMPLATE.Replace(loginBodyParams);

        var loginResponse = await _apiRefit.Login(loginBody);
        if (!loginResponse.IsSuccessStatusCode)
        {
            return null;
        }

        // Extract the Set-Cookie header
        if (loginResponse.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            Cookies.Clear();
            Cookies.AddRange(cookies);
        }

        Token = loginResponse.Content.Token;

        return loginResponse.Content;
    }

}
