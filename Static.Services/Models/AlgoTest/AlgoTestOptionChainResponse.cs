using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Static.Services.Models.AlgoTest
{
    public class AlgoTestOptionChainResponse
    {
        public string Candle { get; set; }

        [JsonProperty("cash")]
        public AlgoTestCashDetail Cash { get; set; }

        [JsonProperty("options")]
        public Dictionary<string, AlgoTestOption>? Options { get; set; }

        [JsonProperty("underlying")]
        public string Underlying { get; set; }
    }

    public class AlgoTestCashDetail
    {
        [JsonProperty("close")]
        public string LastPrice { get; set; }
    }

    public class AlgoTestOption
    {
        [JsonProperty("call_close")]
        public List<decimal?> CallClosePrices { get; set; }


        [JsonProperty("call_delta")]
        public List<decimal?> CallDeltas { get; set; }


        [JsonProperty("call_gamma")]
        public List<decimal?> CallGammas { get; set; }


        [JsonProperty("call_implied_fut")]
        public List<decimal?> CallImpliedFutures { get; set; }


        [JsonProperty("call_implied_vol")]
        public List<decimal?> CallImpliedVolumes { get; set; }


        [JsonProperty("call_rho")]
        public List<decimal?> CallRhos { get; set; }


        [JsonProperty("call_theta")]
        public List<decimal?> CallThetas { get; set; }


        [JsonProperty("call_timestamp")]
        public List<string?> CallTimestamps { get; set; }


        [JsonProperty("call_vega")]
        public List<decimal?> CallVegas { get; set; }


        [JsonProperty("put_close")]
        public List<decimal?> PutClosePrices { get; set; }

        [JsonProperty("put_delta")]
        public List<decimal?> PutDeltas { get; set; }


        [JsonProperty("put_gamma")]
        public List<decimal?> PutGammas { get; set; }


        [JsonProperty("put_implied_fut")]
        public List<decimal?> PutImpliedFutures { get; set; }


        [JsonProperty("put_implied_vol")]
        public List<decimal?> PutImpliedVolumes { get; set; }


        [JsonProperty("put_rho")]
        public List<decimal?> PutRhos { get; set; }


        [JsonProperty("put_theta")]
        public List<decimal?> PutThetas { get; set; }


        [JsonProperty("put_timestamp")]
        public List<string?> PutTimestamps { get; set; }


        [JsonProperty("put_vega")]
        public List<decimal?> PutVegas { get; set; }


        [JsonProperty("strike")]
        public List<uint?> Strikes { get; set; }

    }
}
