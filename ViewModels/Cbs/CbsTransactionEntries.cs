using System;

namespace RTGSWebApi.ViewModels.Cbs
{
    public class CbsTransactionEntries
    {
        public string BizMsgIdr { get; set; }
        public string InstrId { get; set; }
        public string FRBICFI { get; set; }
        public string FromRoutingNo { get; set; }
        public string TOBICFI { get; set; }
        public string ToRoutingNo { get; set; }
        public string MserverDate { get; set; }
        public string TtlIntrBkSttlmCurr { get; set; }
        public double TtlIntrBkSttlmAmt { get; set; }
        public string MuserCode { get; set; }
        public string DbtrNm { get; set; }
        public string DbtrAcctNo { get; set; }
        public string CdtrNm { get; set; }
        public string CdtrAcctNo { get; set; }
        public string RmtInfUstrd { get; set; }
        public string InstredAgent { get; set; }

        public string FloraBranchCode { get; set; }

        public string TransectionType { get; set; }

        public string DebitorGLAcc { get; set; }
        public string CreditorGLAcc { get; set; }

        public string CbsTraceNo { get; set; }

        public AccEntity ObjAccounting { get; set; }
        public string CheckNo { get; set; }
        public string CheckDate { get; set; }

        //public Temenosuser objTemenosuser = new Temenosuser();

        public string AllowedATypeCodes { get; set; }
        public string Remrks { get; set; }

        public Int32 Usercode { get; set; }
    }
}
