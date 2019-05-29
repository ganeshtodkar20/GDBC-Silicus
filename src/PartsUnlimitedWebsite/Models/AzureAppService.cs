using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsUnlimited.Models
{
  

    public class Properties
    {
        public string name { get; set; }
        public string workerSize { get; set; }
        public int numberOfWorkers { get; set; }
        public string currentWorkerSize { get; set; }
        public int currentNumberOfWorkers { get; set; }
        public string status { get; set; }
        public int maximumNumberOfWorkers { get; set; }
        public string geoRegion { get; set; }
        public int numberOfSites { get; set; }
        public bool isSpot { get; set; }
        public string kind { get; set; }
        public bool reserved { get; set; }
        public string mdmId { get; set; }
        public int targetWorkerCount { get; set; }
        public int targetWorkerSizeId { get; set; }
        public string provisioningState { get; set; }
    }

    public class Sku
    {
        public string name { get; set; }
        public string tier { get; set; }
        public string size { get; set; }
        public string family { get; set; }
        public int capacity { get; set; }
    }

    public class AzureAppService
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public string kind { get; set; }
        public string location { get; set; }
        public Properties properties { get; set; }
        public Sku sku { get; set; }
    }
}
