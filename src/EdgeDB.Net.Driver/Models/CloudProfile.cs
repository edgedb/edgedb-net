using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDB.Models
{
    internal sealed class CloudProfile
    {
        [JsonProperty("secret_key")]
        public string? SecretKey { get; set; }
    }
}
