using Newtonsoft.Json.Linq;
using Static.Services.Models;

namespace Static.Services.FlatTrade;

public class AdditionalHeadersHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {

        //request.Headers.UserAgent.ParseAdd(FlatTradeApiConstant.USER_AGENT);

        return await base.SendAsync(request, cancellationToken);
    }
}
