
namespace RTGSWebApi.Infrastructure.Utility
{
    public class ConfigurationOptions
    {
        public string RtgsDbServerIpOrName { get; set; }
        public string RtgsDbName { get; set; }
        public string CbsDbServerIpOrName { get; set; }
        public string CbsDbName { get; set; }
        public string Provider { get; set; }
        public string RtgsAccountNo { get; set; }

        public string cbsIntegrationYN { get; set; }
        public int TokenExpiration { get; set; }

        public string CbsVendorType { get; set; }
    }
}