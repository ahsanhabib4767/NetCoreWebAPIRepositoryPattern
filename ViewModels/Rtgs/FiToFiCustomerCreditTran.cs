using RTGSWebApi.ViewModels.Cbs;
using RTGSWebApi.Model;
using System;

namespace RTGSWebApi.ViewModels.Rtgs
{
    public class FiToFiCustomerCreditTran
    {
        public string FRBICFI { get; set; }
        public string TOBICFI { get; set; }

        public string DebitorBankCode { get; set; }
        public string CreditorBankCode { get; set; }

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
        public string BizMsgIdr { get; set; }

        public string InstrId { get; set; }
        public string DbtrAddress { get; set; }
        public string DbtrStreetName { get; set; }
        public string DbtrTownName { get; set; }
        public string DbtrCountry { get; set; }
        public string CdtrCountry { get; set; }
        public string CdtrAddress { get; set; }
        public string CdtrStreetName { get; set; }
        public string CdtrTownName { get; set; }

        public string FromRoutingNo { get; set; }
        public string FloraBranchCode { get; set; }

        public string ToRoutingNo { get; set; }
        public string TransectionType { get; set; }

        public string DebitorGLAcc { get; set; }
        public string CBSTraceNO { get; set; }

        public string CreditorGLAcc { get; set; }
        public string ChargeApplyYN { get; set; }

        public AccEntity ObjAccounting { get; set; }

        public string CheckNo { get; set; }
        public string CheckDate { get; set; }

        public string AllowedInwordATypeCode { get; set; }

        public string AllowedOutwardATypeCode { get; set; }

        public string Action { get; set; }

        public string PostedYN { get; set; }

        public string AppYN { get; set; }
        public string ConfirmYN { get; set; }

        public string Custom_off_code { get; set; }

        public string Reg_year { get; set; }

        public string Reg_Num { get; set; }

        public string Dec_code { get; set; }

        public string Cus_Mob_no { get; set; }
        public string BIN_No { get; set; }
        public string Econonic_Code { get; set; }
        public string UserCode { get; set; }
        public string Password { get; set; }
        public string CbsVendorType { get; set; }

        public static explicit operator FiToFiCustomerCreditTran(VatOnlineFiToFiCustomerCreditTran v)
        {
            throw new NotImplementedException();
        }
    }
}
