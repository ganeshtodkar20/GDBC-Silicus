using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsUnlimited.Models
{
    public class ConfigSettings
    {
        public ConnectionStringsSettings ConnectionStrings { get; set; }
    }

    public class ConnectionStringsSettings
    {
        public string DefaultConnectionString { get; set; }
    }
}
