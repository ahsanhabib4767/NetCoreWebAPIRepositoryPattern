namespace RTGSWebApi.ViewModels.Cbs
{
    public class CbsCustomer
    {
        public string CustomerAccountNo { get; set; }
        public string BranchCode { get; set; }
        public string CustomerName { get; set; }
        public string Module { get; set; }

        public string AtypeCode { get; set; }
        public string Address { get; set; }
        public string Town { get; set; }
        public string Street { get; set; }
        public string Country { get; set; }
        public string DrawableAmount { get; set; }
        public string CurrCode { get; set; }

        public CbsMsg AccountStatus { get; set; }
    }
}
