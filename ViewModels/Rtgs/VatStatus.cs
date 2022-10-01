using System;

namespace RTGSWebApi.ViewModels.Rtgs
{
    public class VatStatus
    {
        public string BizMsgId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string DbtrAcctNo { get; set; }
        public string UserCode { get; set; }
        public string Password { get; set; }
    }
}
