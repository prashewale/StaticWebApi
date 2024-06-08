using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Static.Services.Models
{
    public class ApiKeyModel : IIdentity
    {
        public string ApiKey { get; set; }
        public int Id { get ; set ; }
    }
}
