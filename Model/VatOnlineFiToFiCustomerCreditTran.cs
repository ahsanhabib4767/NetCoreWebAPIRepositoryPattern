using System.Runtime.Serialization;

 namespace RTGSWebApi.Model
{
    public class VatOnlineFiToFiCustomerCreditTran
    {
        
        public string FRBICFI { get; set; }
        
        public string TOBICFI { get; set; }
        
        public string DebitorBankCode { get; set; }
        
        public string CreditorBankCode { get; set; }
        
        public string TrnDate { get; set; }
        
        public string TtlIntrBkSttlmCurr { get; set; }

        
        public double? TtlIntrBkSttlmAmt { get; set; }
        
        public string MuserCode { get; set; }
        
        public string DbtrNm { get; set; }
        
        public string DbtrAcctNo { get; set; }
        
        public string CdtrNm { get; set; }
        
        public string CdtrAcctNo { get; set; }

        
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


        
        public string ToRoutingNo { get; set; }
        
        public string TransactionType { get; set; }
        
        public string CBSTraceNO { get; set; }
        
        public string ChargeApplyYN { get; set; }
        
        public string CheckNo { get; set; }
        
        public string CheckDate { get; set; }

        
        public string Action { get; set; }
        
        public string ConfirmYN { get; set; }
        
        public string RmtInfUstrdBIN { get; set; }
        
        public string RmtInfUstrdecononiccode { get; set; }
        
        public string CbsVendorType { get; set; }
        
        public string FloraBranchCode { get; set; }
        
        public string ChallanNo { get; set; }
        
        public string ChallanRemarks { get; set; }
        
        public string AppCode { get; set; }
        
        public string PaymentStatus { get; set; }
        
        public string UserCode { get; set; }
        
        public string Password { get; set; }
        
        public string RmtInfUstrd { get; set; }
        
        public string BankRefNo { get; set; }
        //-----------------------------
        
        public string messageCode { get; set; }
        
        public string message { get; set; }
        public string CBSYN { get; set; }

       //Pacs008 Extension


        [DataMember]
        public string MserverDate { get; set; }
       

        [DataMember]
        public string TransectionType { get; set; }

       

     
        public string AllowedOutwardATypeCode { get; set; }


        [DataMember]
        public string PostedYN { get; set; }

        [DataMember]
        public string AppYN { get; set; }
        [DataMember]
       
        public string Reg_year { get; set; }

        [DataMember]
        public string Reg_Num { get; set; }

        [DataMember]
        public string Dec_code { get; set; }

        [DataMember]
        public string Cus_Mob_no { get; set;}
        [DataMember]
        public string BIN_No { get; set; }
        [DataMember]
        public string Econonic_Code { get; set; }


        ///////Custom
        public string Custom_off_code { get; set; }

     
    }
}
