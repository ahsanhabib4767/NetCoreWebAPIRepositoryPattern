using System;

namespace RTGSWebApi.ViewModels.Cbs
{
    public class AccEntity
    {
        public string EntryUserId { get; set; }
        public string RtgsGLAccNo { get; set; }
        public string RtgsIbtaAccNo { get; set; }
        public string HOBRCODE { get; set; }
        public string InwardRTGSGlAc { get; set; }
        public string InwardRevRTGSSetac { get; set; }

        public string ParkGLAcNo { get; set; }
        public string TranType { get; set; }
        public string ChrgApplyYN { get; set; }
        public Double ChrgAmt { get; set; }
        public Double IncAmt { get; set; }
        public string IncAccount { get; set; }
        public Double VatAmt { get; set; }
        public string VatAccount { get; set; }
        public string BankName { get; set; }
        public string BranchName { get; set; }

    }
}
