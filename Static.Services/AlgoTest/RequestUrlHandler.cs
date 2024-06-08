using Microsoft.Extensions.Options;
using Refit;
using Static.Services.Models.AlgoTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Static.Services.AlgoTest
{
    class RequestUrlHandler(IOptions<AlgoTestCredential> credentialOptions) : DelegatingHandler
    {
        private readonly AlgoTestCredential _credential = credentialOptions.Value;
        private readonly List<string> _priceHostPaths = new List<string>()
        {
            "/contracts",
            "/trading-calendar",
            "/day-contracts",
            "/ltp",
            "/option-chain",
        };

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri is null)
                throw new Exception("Request Url is not provided.");

            // Get the type of the target interface
            var builder = new UriBuilder(request.RequestUri);
            if(_priceHostPaths.Contains(builder.Path))
            {
                builder.Host = _credential.PriceHost;
            }
            request.RequestUri = builder.Uri;

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
   }
}
