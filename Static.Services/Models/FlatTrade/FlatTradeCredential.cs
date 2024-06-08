using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Static.Services.Models.FlatTrade
{
    public class FlatTradeCredential
    {
        public string ApiUrl { get; set; }
        public string WebSocketUrl { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string PanNumber { get; set; }
        public string ApkVersion { get; set; }
        public string VC { get; set; }
        public string AppKey { get; set; }
        public string Source { get; set; }
        public string UserAgent { get; set; }
    }
}
