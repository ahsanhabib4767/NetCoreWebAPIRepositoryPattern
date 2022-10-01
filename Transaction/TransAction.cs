using RTGSWebApi.Controllers;
using RTGSWebApi.Infrastructure.Utility;
using RTGSWebApi.UnitOfWorks;
using RTGSWebApi.ViewModels.Cbs;
using RTGSWebApi.ViewModels.Rtgs;
using Microsoft.Extensions.Options;
using RTGSWebApi.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RTGSWebApi.ViewModel.Rtgs;

namespace RTGSWebApi.Transaction
{
    public class TransAction : BaseController, ITransActionDP
    {
        public TransAction(IUnitOfWork uow,IUnitOfWorkCbs uowcbs, IOptions<ConfigurationOptions> options)
        {
           
            Uow = uow;
            UowCbs = uowcbs;
            Options = options;
        }

        public async Task<dynamic> PaymentReconciliation(VatStatus entity)
        {
            try
            {
                if (entity == null || entity.DbtrAcctNo == null || entity.DbtrAcctNo.ToString().Trim().Length <= 0)
                    throw new Exception("{\"Msg_Code\": \"-1\",\"Msg\": \"Invalid Request Data.\",\"RTGS_Ref_No\": \"" + "0" + "\",\"Transaction_No.\": \"" + "0" + "\"}");

                var result = await GetVatOnlineInfoByBizMsgIdnDate(entity.BizMsgId,entity.FromDate, entity.ToDate, entity.DbtrAcctNo);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("{\"Msg_Code\": \"-1\",\"Msg\": \"Invalid credential.\",\"RTGS_Ref_No\": \"" + "0" + "\",\"Transaction_No.\": \"" + "0" + "\"}");
            }
        }
        //Vat,TAX,Pacs008,Custom Duty Payment//Ahsan_2021
        public async Task<dynamic> PaymentInstruction(VatOnlineFiToFiCustomerCreditTran entity)
        {
            try
            {
                if (entity == null)
                    return retJsonMsg("-1", "Invalid Request Data", "0", "0");
                else if (entity.DbtrAcctNo == null)
                    return retJsonMsg("-1", "Invalid Request for debitor A/C", "0", "0");
                else if (entity.TtlIntrBkSttlmAmt <= 0)
                    return retJsonMsg("-1", "Amount can't be zero or less", "0", "0");
                else if (entity.DbtrAcctNo.ToString().Trim().Length <= 11)
                    return retJsonMsg("-1", "Invalid Debitor A/c", "0", "0");
                else if (entity.CbsVendorType.ToString().Trim().Length <= 0)
                    return retJsonMsg("-1", "Invalid Request Cbs Vendor", "0", "0");
     
                //var vBranchCode = await GetBranchCodeByAccountNo(entity.DbtrAcctNo);CBS
                //var vBranchCode = "0024";
                var vBranchCode = entity.FloraBranchCode;
                if (vBranchCode != null)
                {
                    //var vTrandate = await TransactionDateFromLogin(vBranchCode);CBS
                    //var vTrandate = "09/20/2020";
                 

                    entity.FloraBranchCode = vBranchCode;
                    var vRoutingNo = await GetRoutingNoByBranchCode(vBranchCode);
                    var vTranDate = await GetServerDateTimeBy(vRoutingNo,vBranchCode); // Transaction Date from RTGS
                    entity.TrnDate = vTranDate[2];

                    //entity.TrnDate = Convert.ToDateTime(vTrandate).ToString("MM/dd/yyyy");
                    if (vRoutingNo != null)
                    {
                        var message = await IsAuthorizationClosingTime(vRoutingNo, vBranchCode); 
                        if (message[0] == "OK")
                        {
                            //var drawableAmount = await GetDrawableAmountByAc(entity.DbtrAcctNo, entity.TtlIntrBkSttlmCurr);
                            //var drawableAmount = 10000000.00;
                            //if (entity.TtlIntrBkSttlmAmt > drawableAmount)
                            //{
                            //    return retJsonMsg("0002", "Insufficient funds. Transaction amount can't be greater-than Drawable Amount", "0", "0");
                            //}
                            if (entity.TransactionType == "VAT" || entity.TransactionType == "TAX")
                            {
                                var retMsg = await SaveVatOnlineFiToFiCustomerCreditTran(entity);
                                if (retMsg[1] == "OK")
                                {
                                    //var resPostBatch = await PostByBatch_VAT(retMsg[3], "101", Convert.ToDateTime(vTrandate), entity.CbsVendorType, entity.FloraBranchCode, entity.ChargeApplyYN);
                                    ////CBS integration
                                    //if (resPostBatch[0] == "OK")
                                    //{
                                    //await AuthByBatch_VAT(retMsg[3], "101", entity.TrnDate, retMsg[2], "");
                                    await AuthByBatch(retMsg[3], "101", entity.TrnDate, retMsg[2], "");
                                    //    await UpdateTraceNo(retMsg[3], resPostBatch[2]);
                                    //}
                                    //No CBS integration
                                     //retMsg[1] == "OK" && entity.TransactionType == "VAT"
                                    
                                        await APiReqLog(entity, retMsg[3], retMsg[1], retMsg[0]);
                                    
                                    return retJsonMsg("1", "Success", retMsg[3].ToString(), retMsg[3].ToString());
                                }
                                return retJsonMsg(retMsg[1].ToString(), retMsg[0].ToString(), "0", "0");
                            } 
                            else if (entity.TransactionType == "Pacs008")
                            {
                                var retMsg = await SaveOrUpdate008IB(entity);
                                if (retMsg[1] == "OK")
                                {
                                    await AuthByBatch(retMsg[2], "101", entity.TrnDate, retMsg[2], "");
                                    await APiReqLog(entity, retMsg[2], retMsg[1], retMsg[0]);
                                   
                                    return retJsonMsg("1", "Success", retMsg[2].ToString(), retMsg[2].ToString());
                                }
                                return retJsonMsg(retMsg[1].ToString(), retMsg[0].ToString(), "0", "0");
                            }
                            else if (entity.TransactionType == "CustomPc008")
                            {
                                var retMsg = await SaveOrUpdateCustomDuty(entity);
                                if (retMsg[1] == "OK")
                                {
                                    await AuthByBatch(retMsg[2], "101", entity.TrnDate, retMsg[2], "");
                                    await APiReqLog(entity, retMsg[2], retMsg[1], retMsg[0]);

                                    return retJsonMsg("1", "Success", retMsg[2].ToString(), retMsg[2].ToString());
                                }
                                return retJsonMsg(retMsg[1].ToString(), retMsg[0].ToString(), "0", "0");
                            }
                            return retJsonMsg("-1", "Invalid transaction type", "0", "0");
                            //else if (entity.TransactionType == "TAX")
                            //{
                            //    var retMsg = await SaveOrUpdateEchallan(entity);
                            //    if (retMsg[1] == "OK")
                            //    {

                            //        await APiReqLog(entity, retMsg[2], retMsg[1], retMsg[0]);

                            //        return retJsonMsg("1", "Success", retMsg[2].ToString(), retMsg[2].ToString());
                            //    }
                            //    return retJsonMsg(retMsg[1].ToString(), retMsg[0].ToString(), "0", "0");
                            //}
                            //return retJsonMsg("-1", "Invalid transaction type", "0", "0");
                        }
                        return retJsonMsg("-1", "RTGS Transaction Time is over", "0", "0");
                    }
                    return retJsonMsg("-1", "Invalid Debtor Branch", "0", "0");
                }
                return retJsonMsg("-1", "Invalid Account No", "0", "0");
            }
            catch (Exception ex)
            {
                return retJsonMsg("-1", ex.Message, "0", "0");
            }
        }
        public async Task<string[]> SaveVatOnlineFiToFiCustomerCreditTran(VatOnlineFiToFiCustomerCreditTran objInstr)
        {
            string[] messages = new string[4];
            try
            {
                //DbCommand command = _dbContextRTGS.ExecuteStoredProcesure("sp_ins_FItoFIVatOnlineCredittrf");
                var dbCommand = Uow.DbStoredProcedure("sp_ins_FItoFIVatOnlineCredittrf");
                try
                {
                    Uow.AddInParameter(dbCommand, "VFRBICFI", DbType.String, 9, CheckWhiteSpaceAndSpecilChar(objInstr.FRBICFI));
                    Uow.AddInParameter(dbCommand, "VFromRoutingNo", DbType.String, 9, CheckWhiteSpaceAndSpecilChar(objInstr.FromRoutingNo));
                    Uow.AddInParameter(dbCommand, "VDebitorBankCode", DbType.String, 5, CheckWhiteSpaceAndSpecilChar(objInstr.DebitorBankCode));

                    Uow.AddInParameter(dbCommand, "VTOBICFI", DbType.String, 9, CheckWhiteSpaceAndSpecilChar(objInstr.TOBICFI));
                    Uow.AddInParameter(dbCommand, "VToRoutingNo", DbType.String, 9, CheckWhiteSpaceAndSpecilChar(objInstr.ToRoutingNo));
                    Uow.AddInParameter(dbCommand, "VCreditorBankCode", DbType.String, 5, CheckWhiteSpaceAndSpecilChar(objInstr.CreditorBankCode));

                    Uow.AddInParameter(dbCommand, "Vmserverdate", DbType.String, 12, CheckWhiteSpaceAndSpecilChar(objInstr.TrnDate));
                    Uow.AddInParameter(dbCommand, "VTtlIntrBkSttlmCurr", DbType.String, 20, CheckWhiteSpaceAndSpecilChar(objInstr.TtlIntrBkSttlmCurr));
                    Uow.AddInParameter(dbCommand, "VTtlIntrBkSttlmAmt", DbType.String, 20, objInstr.TtlIntrBkSttlmAmt);
                    Uow.AddInParameter(dbCommand, "Vmuser_code", DbType.String, 5, "101"); //CheckWhiteSpaceAndSpecilChar(objInstr.MuserCode));
                    Uow.AddInParameter(dbCommand, "VDbtrNm", DbType.String, 20, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrNm));
                    Uow.AddInParameter(dbCommand, "VDbtrAddress", DbType.String, 200, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAddress));
                    Uow.AddInParameter(dbCommand, "VDbtrStreet", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrStreetName));
                    Uow.AddInParameter(dbCommand, "VDbtrTown", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrTownName));
                    Uow.AddInParameter(dbCommand, "VDbtrCtry", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrCountry));
                    Uow.AddInParameter(dbCommand, "VDbtrAcctNo", DbType.String, 17, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAcctNo.Trim()));
                    Uow.AddInParameter(dbCommand, "VCdtrNm", DbType.String, 170, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrNm));

                    Uow.AddInParameter(dbCommand, "VCdtrAddress", DbType.String, 200, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrAddress));
                    Uow.AddInParameter(dbCommand, "VCdtrStreet", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrStreetName));
                    Uow.AddInParameter(dbCommand, "VCdtrTown", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrTownName));
                    Uow.AddInParameter(dbCommand, "VCdtrCtry", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrCountry));

                    Uow.AddInParameter(dbCommand, "VCdtrAcctNo", DbType.String, 17, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrAcctNo));
                    Uow.AddInParameter(dbCommand, "VRmtInfUstrd", DbType.String, 50, CheckWhiteSpaceAndSpecilChar(objInstr.RmtInfUstrdecononiccode));
                    Uow.AddInParameter(dbCommand, "VCheckNo", DbType.String, 7, CheckWhiteSpaceAndSpecilChar(objInstr.CheckNo));
                    Uow.AddInParameter(dbCommand, "VCheckDate", DbType.String, 12, CheckWhiteSpaceAndSpecilChar(objInstr.CheckDate));
                    Uow.AddInParameter(dbCommand, "VChargeApplyYN", DbType.String, 1, objInstr.ChargeApplyYN);

                    Uow.AddInParameter(dbCommand, "VBIN_No", DbType.String, 50, objInstr.RmtInfUstrdBIN);
                    Uow.AddInParameter(dbCommand, "VEcononic_Code", DbType.String, 50, objInstr.RmtInfUstrdecononiccode);
                    Uow.AddInParameter(dbCommand, "VFloraBranchCode", DbType.String, 4, objInstr.FloraBranchCode);

                    Uow.AddOutParameter(dbCommand, "Vmsg_code", DbType.String, 5, "101");
                    Uow.AddOutParameter(dbCommand, "VMSG", DbType.String, 200, "0");
                    Uow.AddOutParameter(dbCommand, "VoutInstrId", DbType.String, 35, "0");
                    Uow.AddOutParameter(dbCommand, "VoutBizMsgIdr", DbType.String, 35, "0");

                    await Uow.TblFiToFiCustomerCreditTranRepository.ExecuteStoredProc(dbCommand);
                    var cmd = (DbCommand)dbCommand;

                    messages[0] = cmd.Parameters["VMSG"].Value.ToString();
                    messages[1] = cmd.Parameters["Vmsg_code"].Value.ToString();
                    messages[2] = cmd.Parameters["VoutInstrId"].Value.ToString();
                    messages[3] = cmd.Parameters["VoutBizMsgIdr"].Value.ToString();
                }

                catch (Exception ex)
                {
                    throw ex;
                }
                return messages;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<dynamic> PaymentInstruction2(VatOnlineFiToFiCustomerCreditTran entity)
        {
            try
            {
                if (entity == null)
                    return retJsonMsg("-1", "Invalid Request Data", "0", "0");
                else if (entity.DbtrAcctNo == null)
                    return retJsonMsg("-1", "Invalid Request for debitor A/C", "0", "0");
                else if (entity.TtlIntrBkSttlmAmt <= 0)
                    return retJsonMsg("-1", "Amount can't be zero or less", "0", "0");
                else if (entity.DbtrAcctNo.ToString().Trim().Length <= 11)
                    return retJsonMsg("-1", "Invalid Debitor A/c", "0", "0");
                else if (entity.CbsVendorType.ToString().Trim().Length <= 0)
                    return retJsonMsg("-1", "Invalid Request Cbs Vendor", "0", "0");
                var vBranchCode = entity.FloraBranchCode;
                if (vBranchCode != null)
                {
                    entity.FloraBranchCode = vBranchCode;
                    var vRoutingNo = await GetRoutingNoByBranchCode(vBranchCode);
                    var vTranDate = await GetServerDateTimeBy(vRoutingNo, vBranchCode); // Transaction Date from RTGS
                    entity.TrnDate = vTranDate[2];
                    if (vRoutingNo != null)
                    {
                        var message = await IsAuthorizationClosingTime(vRoutingNo, vBranchCode);
                        if (message[0] == "OK")
                        {
                                var retMsg = await SaveData(entity);
                                if (retMsg[1] == "OK")
                                {
                                    
                                    await AuthByBatch(retMsg[2], "101", entity.TrnDate, retMsg[2], "");
                                    await APiReqLog(entity, retMsg[3], retMsg[1], retMsg[0]);
                                    return retJsonMsg("1", "Success", retMsg[3].ToString(), retMsg[3].ToString());
                                }
                                return retJsonMsg(retMsg[1].ToString(), retMsg[0].ToString(), "0", "0");
                           
                            
                        }
                        return retJsonMsg("-1", "RTGS Transaction Time is over", "0", "0");
                    }
                    return retJsonMsg("-1", "Invalid Debtor Branch", "0", "0");
                }
                return retJsonMsg("-1", "Invalid Account No", "0", "0");

                //return retJsonMsg("-1", "Invalid transaction type", "0", "0");
            }
            catch (Exception ex)
            {
                return retJsonMsg("-1", ex.Message, "0", "0");
            }
        }

        public async Task<string[]> SaveOrUpdateCustomDuty(VatOnlineFiToFiCustomerCreditTran objInstr)
        {
            string[] messages = new string[3];
            try
            {
                
                var _dbContextRTGS = Uow.DbStoredProcedure("sp_ins_FItoFICustomDutyIB");

                try
                {
                    Uow.AddInParameter(_dbContextRTGS, "VFRBICFI", DbType.String, 9, CheckWhiteSpaceAndSpecilChar(objInstr.FRBICFI));

                    Uow.AddInParameter(_dbContextRTGS,"VFromRoutingNo", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.FromRoutingNo));
                    Uow.AddInParameter(_dbContextRTGS, "VFromFloraCode", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.FloraBranchCode));
                    Uow.AddInParameter(_dbContextRTGS, "VDebitorBankCode", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DebitorBankCode));

                    Uow.AddInParameter(_dbContextRTGS, "VTOBICFI", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.TOBICFI));
                    Uow.AddInParameter(_dbContextRTGS, "VToRoutingNo", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.ToRoutingNo));
                    Uow.AddInParameter(_dbContextRTGS, "VCreditorBankCode", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.CreditorBankCode));

                    Uow.AddInParameter(_dbContextRTGS, "Vmserverdate", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.TrnDate));
                    Uow.AddInParameter(_dbContextRTGS, "VTtlIntrBkSttlmCurr", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.TtlIntrBkSttlmCurr));
                    Uow.AddInParameter(_dbContextRTGS, "VTtlIntrBkSttlmAmt", DbType.String, 25, objInstr.TtlIntrBkSttlmAmt);
                    Uow.AddInParameter(_dbContextRTGS, "Vmuser_code", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.UserCode));
                    //Debit
                    Uow.AddInParameter(_dbContextRTGS, "VDbtrNm", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.DbtrNm));
                    Uow.AddInParameter(_dbContextRTGS, "VDbtrAddress", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAddress));
                    Uow.AddInParameter(_dbContextRTGS, "VDbtrStreet", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.DbtrStreetName));
                    Uow.AddInParameter(_dbContextRTGS, "VDbtrTown", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.DbtrTownName));
                    Uow.AddInParameter(_dbContextRTGS, "VDbtrCtry", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.DbtrCountry));
                    Uow.AddInParameter(_dbContextRTGS, "VDbtrAcctNo", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAcctNo.Trim()));
                    //Credit
                    Uow.AddInParameter(_dbContextRTGS, "VCdtrNm", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.CdtrNm));
                    Uow.AddInParameter(_dbContextRTGS, "VCdtrAddress", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.CdtrAddress));
                    Uow.AddInParameter(_dbContextRTGS, "VCdtrStreet", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.CdtrStreetName));
                    Uow.AddInParameter(_dbContextRTGS, "VCdtrTown", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.CdtrTownName));
                    Uow.AddInParameter(_dbContextRTGS, "VCdtrCtry", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.CdtrCountry));

                    Uow.AddInParameter(_dbContextRTGS, "VCdtrAcctNo", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.CdtrAcctNo));
                    Uow.AddInParameter(_dbContextRTGS, "VRmtInfUstrd", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.RmtInfUstrd));
                    Uow.AddInParameter(_dbContextRTGS, "VCheckNo", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.CheckNo));
                    Uow.AddInParameter(_dbContextRTGS, "VCheckDate", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.CheckDate));
                    Uow.AddInParameter(_dbContextRTGS, "VChargeApplyYN", DbType.String, 25, objInstr.ChargeApplyYN);

                    #region Custom Duty
                    Uow.AddInParameter(_dbContextRTGS, "VCustom_off_code", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.Custom_off_code));
                    Uow.AddInParameter(_dbContextRTGS, "VReg_year", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.Reg_year));
                    Uow.AddInParameter(_dbContextRTGS, "VReg_Num", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.Reg_Num));
                    Uow.AddInParameter(_dbContextRTGS, "VDec_code", DbType.String, 25,CheckWhiteSpaceAndSpecilChar(objInstr.Dec_code));
                    Uow.AddInParameter(_dbContextRTGS, "VCus_Mob_no", DbType.String, 25, objInstr.Cus_Mob_no);
                    #endregion Custom Duty



                    Uow.AddOutParameter(_dbContextRTGS, "Vmsg_code", DbType.String, 5, "");
                    Uow.AddOutParameter(_dbContextRTGS, "VMSG", DbType.String, 200, "");
                    Uow.AddOutParameter(_dbContextRTGS, "VoutBizMsgIdr", DbType.String, 35, "");
                    await Uow.TblFiToFiCustomerCreditTranRepository.ExecuteStoredProc(_dbContextRTGS);
                    var cmd = (DbCommand)_dbContextRTGS;

                    messages[0] = cmd.Parameters["VMSG"].Value.ToString();
                    messages[1] = cmd.Parameters["Vmsg_code"].Value.ToString();
                    messages[2] = cmd.Parameters["VoutBizMsgIdr"].Value.ToString();
                    return messages;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        //private async Task<double> GetDrawableAmountByAc(string DbtrAcctNo, string TtlIntrBkSttlmCurr)
        //{
        //    var objAypeCodeList = await GetAllAtypeCode();
        //    if (objAypeCodeList != null)
        //    {
        //        // HttpContext.Current.Session["AllATypeCodeList"] = objAypeCodeList;
        //    }

        //    var drawableAmount = 0.0;
        //    try
        //    {
        //        if (Options.Value.CbsVendorType == "FSL")
        //        {
        //            //If valid Atype then go to CBS
        //            //var obj = await GetCustomerAccountInfoTemp(DbtrAcctNo, "Y", "N");
        //            if (obj.AccountStatus.MsgCode != "ERR")
        //            {
        //                if (obj.CurrCode.Trim() != TtlIntrBkSttlmCurr.Trim())
        //                {
        //                    throw new Exception(retJsonMsg("0016", "OOps! Sorry, Currency Code Mismatch", "0", "0"));
        //                }
        //                else
        //                {
        //                    await IsValidOutwardATypeCodeByCbsAtypeCode_VAT(DbtrAcctNo, obj.AtypeCode, Options.Value.CbsVendorType);
        //                    drawableAmount = Convert.ToDouble(obj.DrawableAmount);
        //                }
        //            }
        //            else
        //            {
        //                throw new Exception(retJsonMsg("0017", "OOps! Sorry, Account <b>[" + DbtrAcctNo + "]</b> not found in CBS Or account not in <b>OPERATIVE</b>. Only Operative accounts are allowed for this transaction", "0", "0"));
        //            }
        //        }

        //        ////for other cbs
        //        //else if (Options.Value.CbsVendorType == "T24" && Global.T24path != "" && entity.FloraBranchCode != "")                
        //        //{
        //        //    var obj = cbsService.GetT24_CustomerAccountInfoBy(entity.DbtrAcctNo, "SELECT",
        //        //        RtgsSession.FloraBranchCode, Global.T24path, RtgsSession.BankCode);

        //        //    if (obj != null)
        //        //    {
        //        //        if (obj.ErrorTitle == "")
        //        //        {
        //        //            drawableAmount = Convert.ToDouble(obj.DrawableAmount);
        //        //        }
        //        //        else
        //        //        {
        //        //            throw new Exception(obj.ErrorTitle);
        //        //        }
        //        //    }
        //        //    else
        //        //    {
        //        //        throw new Exception("{\"Msg_Code\": \"0017\",\"Msg\":Account Not Found In T24!\",\"RTGS_Ref_No\": \"" + "0" + "\",\"Transaction_No.\": \"" + "0" + "\"}");
        //        //    }
        //        //}

        //        else
        //        {
        //            throw new Exception(retJsonMsg("0015", "Invalid CBS Vendor", "0", "0"));
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    return drawableAmount;
        //}

        //public async Task<dynamic> FiCustomerCreditTransfer(FiToFiCustomerCreditTran entity)
        //{
        //    try
        //    {
        //        //var vBranchCode = await GetBranchCodeByAccountNo(entity.DbtrAcctNo);
        //        if (vBranchCode != null)
        //        {
        //            entity.FloraBranchCode = vBranchCode;
        //            var vRoutingNo = await GetRoutingNoByBranchCode(vBranchCode);

        //            if (vRoutingNo != null)
        //            {
        //                entity.FromRoutingNo = vRoutingNo;
        //                var message = await IsPostingTimeOvered(vRoutingNo, vBranchCode, ""); //new Change (need posting time)
        //                if (message[0] == "OK")
        //                {
        //                    var drawableAmount = await GetDrawableAmountByAc(entity.DbtrAcctNo, entity.TtlIntrBkSttlmCurr);
        //                    if (entity.TtlIntrBkSttlmAmt > drawableAmount)
        //                    {
        //                        return retJsonMsg("0002", "Insufficient funds. Transaction amount can't be greater-than Drawable Amount", "0", "0");
        //                    }
        //                    var retMsg = await SaveOrUpdate(entity);
        //                    if (retMsg[1] == "OK")
        //                    {
        //                        return retJsonMsg("0000", "Success", retMsg[3].ToString(), retMsg[1].ToString());
        //                    }
        //                    return retJsonMsg(retMsg[1].ToString(), retMsg[0].ToString(), "0", "0");
        //                }
        //                return retJsonMsg("0003", "Invalid transaction type.", "0", "0");
        //            }
        //            return retJsonMsg("0013", "RTGS Transaction Time is over", "0", "0");
        //        }
        //        return retJsonMsg("0004", "Invalid Debtor Branch", "0", "0");
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.Message;
        //    }
        //}
        private dynamic retJsonMsg(string msgCode, string msg, string rtgsRefNo, string trnNo)
        {
            return new
            {
                Msg_Code = msgCode,
                Msg = msg,
                RTGS_Ref_No = rtgsRefNo,
                Transaction_No = trnNo
            };
        }
        public async Task<string[]> SaveOrUpdate008IB(VatOnlineFiToFiCustomerCreditTran objInstr)
        {
            string[] messages = new string[3];
            var vRoutingNo = await GetRoutingNoByBranchCode(objInstr.FloraBranchCode);
            try
            {
   
                var dbCommand = Uow.DbStoredProcedure("sp_ins_FItoFICusCredittrf_IB");

                try
                {
                    Uow.AddInParameter(dbCommand, "VFromRoutingNo", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(vRoutingNo));
                    Uow.AddInParameter(dbCommand, "VFlora_Code", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.FloraBranchCode));
                    Uow.AddInParameter(dbCommand, "VToRoutingNo", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.ToRoutingNo));
                    Uow.AddInParameter(dbCommand, "VCreditorBankCode", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CreditorBankCode));
                    Uow.AddInParameter(dbCommand, "VTtlIntrBkSttlmCurr", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.TtlIntrBkSttlmCurr));
                    Uow.AddInParameter(dbCommand, "VTtlIntrBkSttlmAmt", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.TtlIntrBkSttlmAmt));
                    Uow.AddInParameter(dbCommand, "Vmuser_code", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.UserCode));
                    Uow.AddInParameter(dbCommand, "VDbtrNm", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrNm));
                    Uow.AddInParameter(dbCommand, "VDbtrAddress", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAddress));
                    Uow.AddInParameter(dbCommand, "VDbtrStreet", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrStreetName));
                    Uow.AddInParameter(dbCommand, "VDbtrTown", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrTownName));
                    Uow.AddInParameter(dbCommand, "VDbtrCtry", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrCountry));
                    Uow.AddInParameter(dbCommand, "VDbtrAcctNo", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAcctNo.Trim()));
                    Uow.AddInParameter(dbCommand, "VCdtrNm", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrNm));
                    Uow.AddInParameter(dbCommand, "VCdtrAddress", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrAddress));
                    Uow.AddInParameter(dbCommand, "VCdtrStreet", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrStreetName));
                    Uow.AddInParameter(dbCommand, "VCdtrTown", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrTownName));
                    Uow.AddInParameter(dbCommand, "VCdtrCtry", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrCountry));
                    Uow.AddInParameter(dbCommand, "VCdtrAcctNo", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrAcctNo));
                    Uow.AddInParameter(dbCommand, "VRmtInfUstrd", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.RmtInfUstrd));
                    Uow.AddInParameter(dbCommand, "VChargeApplyYN", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.ChargeApplyYN));

                    ////////Out/////////
                    Uow.AddOutParameter(dbCommand, "Vmsg_code", DbType.String, 5, "");
                    Uow.AddOutParameter(dbCommand, "VMSG", DbType.String, 200, "");
                    Uow.AddOutParameter(dbCommand, "VoutBizMsgIdr", DbType.String, 35, "");
                    await Uow.TblFiToFiCustomerCreditTranRepository.ExecuteStoredProc(dbCommand);
                    var cmd = (DbCommand)dbCommand;

                    messages[0] = cmd.Parameters["VMSG"].Value.ToString();
                    messages[1] = cmd.Parameters["Vmsg_code"].Value.ToString();
                    messages[2] = cmd.Parameters["VoutBizMsgIdr"].Value.ToString();
                    return messages;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<string[]> SaveOrUpdateEchallan(VatOnlineFiToFiCustomerCreditTran objInstr)
        {
            string[] messages = new string[4];
            var vRoutingNo = await GetRoutingNoByBranchCode(objInstr.FloraBranchCode);
            try
            {

                var dbCommand = Uow.DbStoredProcedure("sp_ins_FItoFIEchallanCredittrf_IB");

                try
                {
                    Uow.AddInParameter(dbCommand, "VFromRoutingNo", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(vRoutingNo));
                    Uow.AddInParameter(dbCommand, "VFromFloraCode", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.FloraBranchCode));
                    Uow.AddInParameter(dbCommand, "Vmserverdate", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.TrnDate));
                    Uow.AddInParameter(dbCommand, "Vmuser_code", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.UserCode));
                    Uow.AddInParameter(dbCommand, "VTtlIntrBkSttlmAmt", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.TtlIntrBkSttlmAmt));
                    
                   Uow.AddInParameter(dbCommand, "VDbtrNm", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrNm));
                    Uow.AddInParameter(dbCommand, "VDbtrAddress", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAddress));
                    Uow.AddInParameter(dbCommand, "VDbtrStreet", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrStreetName));
                    Uow.AddInParameter(dbCommand, "VDbtrTown", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrTownName));
                    Uow.AddInParameter(dbCommand, "VDbtrCtry", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrCountry));
                    Uow.AddInParameter(dbCommand, "VDbtrAcctNo", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAcctNo));
                    Uow.AddInParameter(dbCommand, "VCdtrAddress", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrAddress));
                    Uow.AddInParameter(dbCommand, "VCdtrStreet", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrStreetName));

                    Uow.AddInParameter(dbCommand, "VCdtrTown", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrTownName));
                    Uow.AddInParameter(dbCommand, "VCdtrCtry", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrCountry));
                    Uow.AddInParameter(dbCommand, "VRmtInfUstrd", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.RmtInfUstrd));


                    Uow.AddInParameter(dbCommand, "VCheckNo", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CheckNo));
                    Uow.AddInParameter(dbCommand, "VCheckDate", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.CheckDate));
                    Uow.AddInParameter(dbCommand, "VChargeApplyYN", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.ChargeApplyYN));

                    Uow.AddInParameter(dbCommand, "VCtgyPrtry", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.TransectionType));
                    Uow.AddInParameter(dbCommand, "VBIN_No", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.RmtInfUstrdBIN));
                    Uow.AddInParameter(dbCommand, "VEcononic_Code", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.RmtInfUstrdecononiccode));

                    ////////Out/////////
                    Uow.AddOutParameter(dbCommand, "Vmsg_code", DbType.String, 5, "");
                    Uow.AddOutParameter(dbCommand, "VMSG", DbType.String, 200, "");
                    Uow.AddOutParameter(dbCommand, "VoutBizMsgIdr", DbType.String, 35, "");
                    Uow.AddOutParameter(dbCommand, "VoutInstrId", DbType.String, 35, "");
                    await Uow.TblFiToFiCustomerCreditTranRepository.ExecuteStoredProc(dbCommand);
                    var cmd = (DbCommand)dbCommand;

                    messages[0] = cmd.Parameters["VMSG"].Value.ToString();
                    messages[1] = cmd.Parameters["Vmsg_code"].Value.ToString();
                    messages[2] = cmd.Parameters["VoutBizMsgIdr"].Value.ToString();
                    messages[3] = cmd.Parameters["VoutInstrId"].Value.ToString();
                    
                    return messages;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static object CheckWhiteSpaceAndSpecilChar(object obj)
        {
            try
            {
                Regex rgq = new Regex(@"[~`!@#$%^&*()+=|\{}':;.,<>/?[\]""_-]");
                // var specialCharVal =ConfigurationSettings.AppSettings["SpecialCharactersNotAllowed"].ToString();

                if (obj == null) return null;
                var objValue = obj.ToString().Trim();
                //if (rgq.IsMatch(objValue))
                // {
                //    throw new Exception("Special Chareacters () are not allowed in RTGS");
                //    // return false;
                // }
                // else
                // {
                //     return true;
                // }
                return string.IsNullOrEmpty(objValue) ? null : objValue;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<dynamic> ApiRequestLog(string postParam, string channelId, string uniqueId, string error_code, string reqMethod)
        {
            string[] messages = new string[2];
            var mgs = "";
            var dbCmd = Uow.DbStoredProcedure("sp_ins_api_log");
            try
            {

                Uow.AddInParameter(dbCmd, "@ReqMsg", DbType.String, 5000, postParam);
                Uow.AddInParameter(dbCmd, "@ChannelId", DbType.String, 5, channelId);
                Uow.AddInParameter(dbCmd, "@UniqueId", DbType.String, 30, uniqueId);
                Uow.AddInParameter(dbCmd, "@reqMethod", DbType.String, 40, reqMethod);
                Uow.AddInParameter(dbCmd, "@Error_code", DbType.String, 10, error_code);
                Uow.AddOutParameter(dbCmd, "@msg", DbType.String, 200, "");
                Uow.AddOutParameter(dbCmd, "@msg_code", DbType.String, 5, "");
                var result = await Uow.TblATypeCodeEntityRepository.ExecuteStoredProc(dbCmd);
                var cmd = (DbCommand)dbCmd;
                messages[0] = cmd.Parameters["@msg_code"].Value.ToString();
                messages[1] = cmd.Parameters["@msg"].Value.ToString();
                if (messages[0] == "01")
                {
                    mgs = messages[1];
                }
                else if (messages[0] == "02")
                {
                    mgs = messages[1];
                }
                else if (messages[0] == "03")
                {
                    mgs = messages[1];
                }
                return mgs;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public OCbsCustomer GetT24_CustomerAccountInfoBy(string customerAccountNo, string input_type, string branch_code, string t24path, string BankCode)
        //{
        //    var objCust = new OCbsCustomer();
        //    try
        //    {
        //        var aCus = new OCbsCustomer();

        //        if (t24path != "")
        //        {
        //            aCus = _TemenosCommonServiceManager.GetT24_CustomerAccountInfoBy(customerAccountNo, input_type, branch_code, t24path, BankCode);
        //            if (aCus == null) return null;
        //            objCust.CustomerAccountNo = aCus.CustomerAccountNo;
        //            objCust.CustomerName = aCus.CustomerName;
        //            objCust.Address = aCus.Address;
        //            objCust.Street = aCus.Street;
        //            objCust.Town = aCus.Town;
        //            objCust.DrawableAmount = aCus.DrawableAmount;
        //            objCust.CurrCode = aCus.CurrCode;
        //            objCust.ErrorTitle = aCus.ErrorTitle.ToString().Trim();
        //        }

        //        return objCust;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}
        //public async Task<dynamic> TransactionDateFromLogin(string userBranchId)
        //{
        //    try
        //    {
        //        DateTime transactiondate = DateTime.Now;
        //        var sqlQuery = "SELECT MIN(dayend_dt) as dDate FROM dayend_date WHERE holyday_yn='NO' AND dayend_yn='NO' AND branch_code='" + userBranchId + "'";
        //        transactiondate = await UowCbs.TblCbsMsgRepository.GetResultByExecuteText(sqlQuery);
        //        var res = Convert.ToDateTime(transactiondate).ToString("MM/dd/yyyy");
        //        return res;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        public async Task<dynamic> GetRoutingNoByBranchCode(string branchCode)
        {
            string selectQuery = "SELECT routingno FROM dbo.Parameter_Branch  where flora_code='" + branchCode + "'";
            try
            {
                return await Uow.TblVatOnlineInfoRepository.GetResultByExecuteText(selectQuery);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Server Time RTGS -Ahsan_2020
        public async Task<string[]> GetServerDateTimeBy(string routingNo, string floracode)
        {
            string[] messages = new string[3];

            try
            {
                //DbCommand command = _dbContextRTGS.ExecuteStoredProcesure("sp_ins_FItoFIVatOnlineCredittrf");
                var dbCommand = Uow.DbStoredProcedure("SP_Get_RTGS_TrnDateByRoutingNo");
                try
                {
                    Uow.AddInParameter(dbCommand, "VRoutingNo", DbType.String, 17, CheckWhiteSpaceAndSpecilChar(routingNo));
                    Uow.AddInParameter(dbCommand, "Vflora_code", DbType.String, 17, CheckWhiteSpaceAndSpecilChar(floracode));
                    Uow.AddOutParameter(dbCommand, "VTDate", DbType.String, 20,"");
                    Uow.AddOutParameter(dbCommand, "Vmsg_code", DbType.String, 10, "");
                    Uow.AddOutParameter(dbCommand, "VMSG", DbType.String, 200, "");
                    await Uow.TblFiToFiCustomerCreditTranRepository.ExecuteStoredProc(dbCommand);
                    var cmd = (DbCommand)dbCommand;

                    messages[0] = cmd.Parameters["VMSG"].Value.ToString();
                    messages[1] = cmd.Parameters["Vmsg_code"].Value.ToString();
                    messages[2] = cmd.Parameters["VTDate"].Value.ToString();
                    string rtgsTrndate = messages[2];
                    if (messages[0] == "OK" && !string.IsNullOrEmpty(rtgsTrndate))
                    {
                        try
                        {

                            var res =  Convert.ToDateTime(rtgsTrndate);
                            
                            
                        }
                        catch (Exception)
                        {
                            throw new Exception("Convertion failed! Invalid Date Format in Dayend table. RTGSTrnDate:[" +
                                                messages[2] + "]");
                        }
                    }
                }

                catch (Exception ex)
                {
                    throw ex;
                }
                return messages;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<dynamic> IsAuthorizationClosingTime(string vRoutingNo, string vBranchCode)
        {
            string[] messages = new string[3];
            try
            {
                var dbCommand = Uow.DbStoredProcedure("sp_IsPostingOrAuthTimeOvered");
                Uow.AddInParameter(dbCommand, "VCheckingFor", DbType.String, 5, "ACT");
                Uow.AddInParameter(dbCommand, "VRoutingNo", DbType.String, 9, vRoutingNo);
                Uow.AddInParameter(dbCommand, "Vflora_code", DbType.String, 4, vBranchCode);
                Uow.AddOutParameter(dbCommand, "VMSG_Code", DbType.String, 5, "");
                Uow.AddOutParameter(dbCommand, "VMSG", DbType.String, 200, "");
                await Uow.TblVatOnlineInfoRepository.ExecuteStoredProc(dbCommand);
                var cmd = (DbCommand)dbCommand;
                messages[0] = cmd.Parameters["VMSG_Code"].Value.ToString();
                messages[1] = cmd.Parameters["VMSG"].Value.ToString();
                cmd.Parameters.Clear();
                cmd.Dispose();
                return messages;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string[]> IsPostingTimeOvered(string routingNo, string floracode, string actionFor)
        {
            string[] messages = new string[3];
            try
            {
                //DbCommand command = _dbContext.ExecuteStoredProcesure("sp_IsPostingOrAuthTimeOvered");
                //command.Parameters.Add(_dbContext.CreateParameter("@VCheckingFor", actionFor));
                //command.Parameters.Add(_dbContext.CreateParameter("@VRoutingNo", routingNo));
                //command.Parameters.Add(_dbContext.CreateParameter("@Vflora_code", floracode));
                //command.Parameters.Add(_dbContext.CreateParameterOut("@VMSG_Code", DbType.String, 5));
                //command.Parameters.Add(_dbContext.CreateParameterOut("@VMSG", DbType.String, 200));

                var dbCommand = Uow.DbStoredProcedure("sp_IsPostingOrAuthTimeOvered");
                Uow.AddInParameter(dbCommand, "VCheckingFor", DbType.String, 5, "PCT");
                Uow.AddInParameter(dbCommand, "VRoutingNo", DbType.String, 9, routingNo);
                Uow.AddInParameter(dbCommand, "Vflora_code", DbType.String, 4, floracode);
                Uow.AddOutParameter(dbCommand, "VMSG_Code", DbType.String, 5, "");
                Uow.AddOutParameter(dbCommand, "VMSG", DbType.String, 200, "");
                await Uow.TblVatOnlineInfoRepository.ExecuteStoredProc(dbCommand);
                var cmd = (DbCommand)dbCommand;
                messages[0] = cmd.Parameters["VMSG_Code"].Value.ToString();
                messages[1] = cmd.Parameters["VMSG"].Value.ToString();
                cmd.Parameters.Clear();
                cmd.Dispose();
                return messages;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string[]> UpdateTraceNo(string bizMsgIdr, string cbsTraceNo)
        {
            string[] messages = new string[3];
            try
            {
                

                var dbCommand = Uow.DbStoredProcedure("sp_UpdateTraceNo");
                Uow.AddInParameter(dbCommand, "VBizMsgIdr", DbType.String, 35,bizMsgIdr);
                Uow.AddInParameter(dbCommand, "VCbsTraceNo", DbType.String, 16,cbsTraceNo);
                //Ahsan
                Uow.AddOutParameter(dbCommand, "Vmsg_code", DbType.String,200,"");
                Uow.AddOutParameter(dbCommand, "VMSG", DbType.String, 200,"");
                await Uow.TblVatOnlineInfoRepository.ExecuteStoredProc(dbCommand);
                var cmd = (DbCommand)dbCommand;
                messages[0] = cmd.Parameters["Vmsg_code"].Value.ToString();
                messages[1] = cmd.Parameters["VMSG"].Value.ToString();
                cmd.Parameters.Clear();
                cmd.Dispose();
                return messages;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       

        
        public async Task<List<FiToFiCustomerCreditTran>> GetInstructionByBizMsgIdForPosting_VAT(string bizMsgIdr)
        {
            List<FiToFiCustomerCreditTran> _objList = new List<FiToFiCustomerCreditTran>();
            String[] messages = new string[3];
            //DbCommand command = _dbContext.ExecuteStoredProcesure("sp_GetInstByBizMsgIdForPosting_VAT");
            //command.Parameters.Add(_dbContext.CreateParameter("VBizMsgIdr", bizMsgIdr));
            //command.Parameters.Add(_dbContext.CreateParameterOut("Vmsg_code", DbType.String, 10));
            //command.Parameters.Add(_dbContext.CreateParameterOut("VMSG", DbType.String, 500));
            //DbDataReader reader = command.ExecuteReader();
            try
            {
                var dbCommand = Uow.DbStoredProcedure("sp_GetInstByBizMsgIdForPosting_VAT");
                Uow.AddInParameter(dbCommand, "@VBizMsgIdr", DbType.String, 35, bizMsgIdr);
                Uow.AddOutParameter(dbCommand, "@Vmsg_code", DbType.String, 10, "");
                Uow.AddOutParameter(dbCommand, "@VMSG", DbType.String, 500, "");
                _objList = (List<FiToFiCustomerCreditTran>)await Uow.TblFiToFiCustomerCreditTranRepository.GetAllExecuteStoredProc(dbCommand);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return _objList;
        }

        public async Task<List<VatOnlineInfo>> GetVatOnlineInfoByBizMsgIdnDate(string bizMsgIdr, DateTime fdate, DateTime tdate, string debitorAccountNo)
        {
            List<VatOnlineInfo> listVatOnline = new List<VatOnlineInfo>();
        
            try
            {
                if (bizMsgIdr == "")
                {
                    bizMsgIdr = "";
                }
                var dbCommand = Uow.DbStoredProcedure("sp_GetVatOnlineInfo");
                Uow.AddInParameter(dbCommand, "Vrtgs_ref", DbType.String, 35, bizMsgIdr);
                Uow.AddInParameter(dbCommand, "Vfdate", DbType.Date, 12, fdate.ToString("MM/dd/yyyy"));
                Uow.AddInParameter(dbCommand, "Vtdate", DbType.Date, 12, tdate.ToString("MM/dd/yyyy"));
                Uow.AddInParameter(dbCommand, "VDebitorAccountNo", DbType.String, 25, debitorAccountNo);
                listVatOnline = (List<VatOnlineInfo>)await Uow.TblVatOnlineInfoRepository.GetAllExecuteStoredProc(dbCommand);
                return listVatOnline;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

        public async Task<string[]> AuthByBatch(string bizMsgIdr, string postingUser, string serverDate, string instrIds, string failedInstrIds)
        {
            string[] messages = new string[3];

            //DbCommand command = _dbContext.ExecuteStoredProcesure("sp_authorizing");
            var dbCommand = Uow.DbStoredProcedure("sp_authorizing");
            try
            {
                Uow.AddInParameter(dbCommand, "VBizMsgIdr", DbType.String, 35, bizMsgIdr);
                Uow.AddInParameter(dbCommand, "VInstrId", DbType.String, 35, instrIds);
                Uow.AddInParameter(dbCommand, "VFailedInstrIds", DbType.String, 35, failedInstrIds);
                Uow.AddInParameter(dbCommand, "Vuser_code", DbType.String, 5, postingUser);
                Uow.AddInParameter(dbCommand, "Vmserverdate", DbType.String, 12, serverDate);
                Uow.AddInParameter(dbCommand, "Vaction", DbType.String, 5, "AB");

                Uow.AddOutParameter(dbCommand, "Vmsg_code", DbType.String, 10, "");
                Uow.AddOutParameter(dbCommand, "VMSG", DbType.String, 500, "");

                await Uow.TblVatOnlineInfoRepository.ExecuteStoredProc(dbCommand);

                var cmd = (DbCommand)dbCommand;
                messages[0] = cmd.Parameters["Vmsg_code"].Value.ToString();
                messages[1] = cmd.Parameters["VMSG"].Value.ToString();
                return messages;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<ATypeCodeEntity>> GetAllAtypeCode()
        {
            try
            {
                //var dt = _dbContext.GenerateDataTable("exec sp_GetRTGS_AtypeCode");
                var dbCommand = Uow.DbStoredProcedure("sp_GetRTGS_AtypeCode");
                var res = (List<ATypeCodeEntity>)await Uow.TblATypeCodeEntityRepository.GetAllExecuteStoredProc(dbCommand);
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsValidOutwardATypeCodeByCbsAtypeCode_VAT(string accountNo, string cbsAtypeCode, string CbsVendorType)
        {
            bool res = false;
            try
            {
                cbsAtypeCode = cbsAtypeCode.Trim();
                switch (CbsVendorType)
                {
                    case "FSL":
                        {
                            if (string.IsNullOrEmpty(cbsAtypeCode))
                                throw new Exception("{\"Msg_Code\": \"0022\",\"Msg\":Error: Sorry! CBS ATypeCode is Null/Empty!\",\"RTGS_Ref_No\": \"" + "0" + "\",\"Transaction_No.\": \"" + "0" + "\"}");
                            if (cbsAtypeCode.Trim().Length == 0)
                                throw new Exception("{\"Msg_Code\": \"0023\",\"Msg\":Error: Sorry! ATypeCode is not setup yet in RTGS. \",\"RTGS_Ref_No\": \"" + "0" + "\",\"Transaction_No.\": \"" + "0" + "\"}");

                            res = true;
                            break;
                        }
                    case "T24":
                        {
                            res = true;
                            break;
                        }
                }
                return await Task.FromResult(res);
            }
            catch (Exception ex)
            {
                throw ex;

            }
        }

        public async Task<string> GetBranchCodeByAccountNo(string accountNo)
        {
            string selectQuery = "SELECT branch_code FROM dbo.cus_ac_1 WHERE accountno='" + accountNo + "'";
            try
            {
                return await UowCbs.TblCbsCustomerRepository.GetResultByExecuteText(selectQuery);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public async Task<dynamic> TransactionDateFromLogin(string userBranchId)
        {
            try
            {
                DateTime transactiondate = DateTime.Now;
                var sqlQuery = "SELECT MIN(dayend_dt) as dDate FROM dayend_date WHERE holyday_yn='NO' AND dayend_yn='NO' AND branch_code='" + userBranchId + "'";
                transactiondate = await UowCbs.TblCbsMsgRepository.GetResultByExecuteText(sqlQuery);
                var res = Convert.ToDateTime(transactiondate).ToString("MM/dd/yyyy");
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private async Task<double> GetDrawableAmountByAc(string DbtrAcctNo, string TtlIntrBkSttlmCurr)
        {
            var objAypeCodeList = await GetAllAtypeCode();
            if (objAypeCodeList != null)
            {
                // HttpContext.Current.Session["AllATypeCodeList"] = objAypeCodeList;
            }

            var drawableAmount = 0.0;
            try
            {
                if (Options.Value.CbsVendorType == "FSL")
                {
                    //If valid Atype then go to CBS
                    var obj = await GetCustomerAccountInfoTemp(DbtrAcctNo, "Y", "N");
                    if (obj.AccountStatus.MsgCode != "ERR")
                    {
                        if (obj.CurrCode.Trim() != TtlIntrBkSttlmCurr.Trim())
                        {
                            throw new Exception(retJsonMsg("0016", "OOps! Sorry, Currency Code Mismatch", "0", "0"));
                        }
                        else
                        {
                            await IsValidOutwardATypeCodeByCbsAtypeCode_VAT(DbtrAcctNo, obj.AtypeCode, Options.Value.CbsVendorType);
                            drawableAmount = Convert.ToDouble(obj.DrawableAmount);
                        }
                    }
                    else
                    {
                        throw new Exception(retJsonMsg("0017", "OOps! Sorry, Account <b>[" + DbtrAcctNo + "]</b> not found in CBS Or account not in <b>OPERATIVE</b>. Only Operative accounts are allowed for this transaction", "0", "0"));
                    }
                }

              
                else
                {
                    throw new Exception(retJsonMsg("0015", "Invalid CBS Vendor", "0", "0"));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return drawableAmount;
        }

        public async Task<CbsCustomer> GetCustomerAccountInfoTemp(string customerAccountNo, string IsOwnCBS, string IsOtherCbsApiIntegration)
        {
            var objCust = new CbsCustomer();
            try
            {
                var aCus = new CbsCustomer();

                if (IsOtherCbsApiIntegration == "N")
                {
                    if ((IsOwnCBS == "Y" || IsOwnCBS == "N"))
                    {
                        aCus = await GetCustomerAccountInfoTemp(customerAccountNo);
                        if (aCus == null) return null;
                        if (aCus.AccountStatus == null) return null;

                        if (aCus.AccountStatus.MsgCode.ToUpper() != "OK")
                        {
                            objCust.AccountStatus = new CbsMsg() { MsgCode = aCus.AccountStatus.MsgCode, Msg = aCus.AccountStatus.Msg };
                        }
                        else
                        {
                            objCust.AccountStatus = new CbsMsg() { MsgCode = aCus.AccountStatus.MsgCode, Msg = aCus.AccountStatus.Msg };
                            objCust.CustomerName = aCus.CustomerName;
                            objCust.Address = aCus.Address;
                            objCust.Town = aCus.Town;
                            objCust.DrawableAmount = aCus.DrawableAmount;
                            objCust.AtypeCode = aCus.AtypeCode;
                            objCust.CurrCode = aCus.CurrCode;
                            objCust.BranchCode = aCus.BranchCode;
                            //objCust.SignatureList = aCus.SignatureList;
                        }
                    }
                }
                else if (IsOtherCbsApiIntegration == "Y")
                {
                }
                return objCust;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CbsCustomer> GetCustomerAccountInfoTemp(string customerAccountNo)
        {
            try
            {
                //DbCommand cmd = _dbContextCBS.ExecuteStoredProcesure("sp_Rtgs_CRM_and_DrawableAmt");
                //cmd.Parameters.Add(_dbContextCBS.CreateParameter("VAccountNo", customerAccountNo));

                var dbCommand = UowCbs.DbStoredProcedure("sp_Rtgs_CRM_and_DrawableAmt");
                UowCbs.AddInParameter(dbCommand, "VAccountNo", DbType.String, 35, customerAccountNo);
                var res = await UowCbs.TblCbsCustomerRepository.GetByExecuteStoredProc(dbCommand);
                return res;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<string[]> PostByBatch_VAT(string bizMsgIdr, string postingUser, DateTime trnDate, string cbsVendorType, string floraBrCode, string chargeYN)
        {
            string[] messages = new string[3];

            try
            {
                var _objInstructionList = await GetInstructionByBizMsgIdForPosting_VAT(bizMsgIdr);
                CbsTransactionEntries _cbstrnEntries = new CbsTransactionEntries();
                //ICbsTrasactionManager _cbstrnManager = new CbsTrasactionManager();
                if (_objInstructionList != null)
                {
                    var noOfInstruction = _objInstructionList.Count;
                    foreach (var objInst in _objInstructionList)
                    {
                        _cbstrnEntries.BizMsgIdr = objInst.BizMsgIdr;
                        _cbstrnEntries.InstrId = objInst.InstrId;
                        _cbstrnEntries.DbtrAcctNo = objInst.TransectionType == RtgsTransectionType.RtgsFICT ? objInst.DebitorGLAcc : objInst.DbtrAcctNo;
                        //_cbstrnEntries.DbtrAcctNo = objInst.DbtrAcctNo;
                        _cbstrnEntries.MserverDate = trnDate.ToString("dd-MMM-yyyy");
                        _cbstrnEntries.MuserCode = postingUser;
                        _cbstrnEntries.RmtInfUstrd = objInst.RmtInfUstrd;
                        _cbstrnEntries.TtlIntrBkSttlmAmt = objInst.TtlIntrBkSttlmAmt;
                        _cbstrnEntries.FloraBranchCode = floraBrCode;
                        _cbstrnEntries.DebitorGLAcc = objInst.DebitorGLAcc;
                        _cbstrnEntries.TransectionType = objInst.TransectionType;
                        _cbstrnEntries.CheckNo = objInst.CheckNo;
                        _cbstrnEntries.CheckDate = objInst.CheckDate;
                        _cbstrnEntries.AllowedATypeCodes = objInst.AllowedOutwardATypeCode;

                        //Added by Motahar
                        _cbstrnEntries.TOBICFI = objInst.ToRoutingNo;
                        //End 

                        _cbstrnEntries.ObjAccounting = new AccEntity();
                        _cbstrnEntries.ObjAccounting.EntryUserId = postingUser;
                        _cbstrnEntries.ObjAccounting.RtgsGLAccNo = objInst.ObjAccounting.RtgsGLAccNo;
                        _cbstrnEntries.ObjAccounting.RtgsIbtaAccNo = objInst.ObjAccounting.RtgsIbtaAccNo;
                        _cbstrnEntries.ObjAccounting.HOBRCODE = objInst.ObjAccounting.HOBRCODE;
                        _cbstrnEntries.ObjAccounting.TranType = objInst.TransectionType;
                        _cbstrnEntries.ObjAccounting.ChrgApplyYN = objInst.ChargeApplyYN;//chargeYN;
                        _cbstrnEntries.ObjAccounting.ChrgAmt = objInst.ObjAccounting.ChrgAmt;
                        _cbstrnEntries.ObjAccounting.IncAmt = objInst.ObjAccounting.IncAmt;
                        _cbstrnEntries.ObjAccounting.IncAccount = objInst.ObjAccounting.IncAccount;
                        _cbstrnEntries.ObjAccounting.VatAmt = objInst.ObjAccounting.VatAmt;
                        _cbstrnEntries.ObjAccounting.VatAccount = objInst.ObjAccounting.VatAccount;

                        try
                        {
                            switch (cbsVendorType)
                            {
                                case "FSL":
                                    messages = await CBSIntegration_VAT(_cbstrnEntries);
                                    break;
                                case "T24":

                                    break;
                                default:
                                    // Nothing to do
                                    // Exception will be Throw autometically
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return messages;
        }
        public async Task<string[]> CBSIntegration_VAT(CbsTransactionEntries objInstr)
        {
            string[] messages = new string[3];
            //DbCommand cmd = _dbContextCBS.ExecuteStoredProcesure("sp_RTGS_DATAINTEG_VAT");

            var dbCommand = UowCbs.DbStoredProcedure("sp_RTGS_DATAINTEG_VAT");
            try
            {
                UowCbs.AddInParameter(dbCommand, "VBizMsgIdr", DbType.String, 35, objInstr.BizMsgIdr);
                UowCbs.AddInParameter(dbCommand, "VInstrId", DbType.String, 35, objInstr.InstrId);
                UowCbs.AddInParameter(dbCommand, "Vtdate", DbType.String, 12, objInstr.MserverDate);
                UowCbs.AddInParameter(dbCommand, "VAccountno", DbType.String, 15, objInstr.DbtrAcctNo);
                UowCbs.AddInParameter(dbCommand, "VAmount_TK", DbType.Double, 20, objInstr.TtlIntrBkSttlmAmt);
                UowCbs.AddInParameter(dbCommand, "Vtrn_branch_code", DbType.String, 4, objInstr.FloraBranchCode);
                UowCbs.AddInParameter(dbCommand, "VPRBRRT", DbType.String, 35, objInstr.TOBICFI.Substring(0, 4) + "/" + objInstr.ToRoutingNo + "/" + objInstr.BizMsgIdr);
                UowCbs.AddInParameter(dbCommand, "Vremarks", DbType.String, 150, objInstr.RmtInfUstrd);
                UowCbs.AddInParameter(dbCommand, "VCheckNo", DbType.String, 20, objInstr.CheckNo);
                UowCbs.AddInParameter(dbCommand, "VCheckDate", DbType.Date, 12, objInstr.CheckDate == "" ? null : objInstr.CheckDate);
                UowCbs.AddInParameter(dbCommand, "VAllowedATypeCodes", DbType.String, 4000, objInstr.AllowedATypeCodes);

                UowCbs.AddInParameter(dbCommand, "VentUser_code", DbType.Int32, 5, objInstr.ObjAccounting.EntryUserId);
                UowCbs.AddInParameter(dbCommand, "VRTGS_glacno", DbType.String, 15, objInstr.ObjAccounting.RtgsGLAccNo);
                UowCbs.AddInParameter(dbCommand, "VRTGS_IBTA_acno", DbType.String, 15, objInstr.ObjAccounting.RtgsIbtaAccNo);
                UowCbs.AddInParameter(dbCommand, "VHOBRCODE", DbType.String, 4, objInstr.ObjAccounting.HOBRCODE);
                UowCbs.AddInParameter(dbCommand, "VLclPrtry", DbType.String, 35, objInstr.ObjAccounting.TranType);
                UowCbs.AddInParameter(dbCommand, "VChrgapplyyn", DbType.String, 1, objInstr.ObjAccounting.ChrgApplyYN);
                UowCbs.AddInParameter(dbCommand, "Vchrgamt", DbType.Double, 20, objInstr.ObjAccounting.ChrgAmt);
                UowCbs.AddInParameter(dbCommand, "Vincamt", DbType.Double, 20, objInstr.ObjAccounting.IncAmt);
                UowCbs.AddInParameter(dbCommand, "Vvatamt", DbType.Double, 20, objInstr.ObjAccounting.VatAmt);
                UowCbs.AddInParameter(dbCommand, "VincAc", DbType.String, 15, objInstr.ObjAccounting.IncAccount);
                UowCbs.AddInParameter(dbCommand, "VVatAc", DbType.String, 15, objInstr.ObjAccounting.VatAccount);

                UowCbs.AddOutParameter(dbCommand, "VMSG", DbType.String, 200, "");
                UowCbs.AddOutParameter(dbCommand, "Vmsg_code", DbType.String, 5, "");
                UowCbs.AddOutParameter(dbCommand, "Vout_traceno", DbType.Double, 16, 0);
                await UowCbs.TblCbsCustomerRepository.ExecuteStoredProc(dbCommand);

                var cmd = (DbCommand)dbCommand;
                messages[0] = cmd.Parameters["Vmsg_code"].Value.ToString();
                messages[1] = cmd.Parameters["VMSG"].Value.ToString();
                messages[2] = cmd.Parameters["Vout_traceno"].Value.ToString();
                cmd.Parameters.Clear();
                cmd.Dispose();
                return messages;
                //messages[0] = cmd.Parameters["Vmsg_code"].Value.ToString();
                //messages[1] = cmd.Parameters["VMSG"].Value.ToString();
                //messages[2] = cmd.Parameters["Vout_traceno"].Value.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<string[]> APiReqLog(VatOnlineFiToFiCustomerCreditTran obj, string BizMsgId, string messagecode, string message)
        {
            //DbCommand command = _dbContext.ExecuteStoredProcesure("Wsp_Rtgs_Api_Req_log");
            var dbCommand = Uow.DbStoredProcedure("Wsp_Rtgs_Api_Req_log");
            string[] messages = new string[3];
            try
            {
                //int result = 0;
                Uow.AddInParameter(dbCommand, "VReqType", DbType.String, 35, obj.TransactionType);
                Uow.AddInParameter(dbCommand, "VBizMsgId", DbType.String, 35,BizMsgId);
                Uow.AddInParameter(dbCommand, "VTDate", DbType.String, 35, obj.TrnDate);
                Uow.AddInParameter(dbCommand, "VReqAcctNo", DbType.String, 35, obj.DbtrAcctNo);
                Uow.AddInParameter(dbCommand, "VReqAmount", DbType.String, 35, obj.TtlIntrBkSttlmAmt);
                Uow.AddInParameter(dbCommand, "VDrBankCode", DbType.String, 35, obj.FromRoutingNo);
                Uow.AddInParameter(dbCommand, "VDrBranchCode", DbType.String, 35, obj.FloraBranchCode);
                Uow.AddInParameter(dbCommand, "VCrBankCode", DbType.String, 35,"");
                Uow.AddInParameter(dbCommand, "VReqRefNo", DbType.String, 35, obj.RmtInfUstrd);
                Uow.AddInParameter(dbCommand, "VBino", DbType.String, 35, obj.RmtInfUstrdBIN);
                Uow.AddInParameter(dbCommand, "VEconicmicno", DbType.String, 35, obj.RmtInfUstrdecononiccode);
                Uow.AddInParameter(dbCommand, "VRemarks", DbType.String, 35, obj.RmtInfUstrd);
                Uow.AddInParameter(dbCommand, "VUser", DbType.String, 35, obj.MuserCode);
                Uow.AddInParameter(dbCommand, "VMsg_Code", DbType.String, 35,messagecode);
                Uow.AddInParameter(dbCommand, "VMSG", DbType.String,500,message);
                
                await Uow.TblVatOnlineInfoRepository.ExecuteStoredProc(dbCommand);

                var cmd = (DbCommand)dbCommand;
                //messages[0] = cmd.Parameters["Vmsg_code"].Value.ToString();
                //messages[1] = cmd.Parameters["VMSG"].Value.ToString();
                cmd.Parameters.Clear();
                cmd.Dispose();
                return messages;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        //Testing_For_ALL_Vat,TAX,Pacs008,Custom Duty Payment
        public async Task<string[]> SaveData(VatOnlineFiToFiCustomerCreditTran objInstr)
        {
            string[] messages = new string[4];
            try
            {
                //DbCommand command = _dbContextRTGS.ExecuteStoredProcesure("sp_ins_FItoFIVatOnlineCredittrf");
                var dbCommand = Uow.DbStoredProcedure("sp_ins_Rtgs_Api_Transfer");
                try
                {   //Transaction Type

                    Uow.AddInParameter(dbCommand, "VTransactionType", DbType.String, 20, CheckWhiteSpaceAndSpecilChar(objInstr.TransactionType));
                    //End
                    Uow.AddInParameter(dbCommand, "VFRBICFI", DbType.String, 9, CheckWhiteSpaceAndSpecilChar(objInstr.FRBICFI));
                    Uow.AddInParameter(dbCommand, "VFromRoutingNo", DbType.String, 9, CheckWhiteSpaceAndSpecilChar(objInstr.FromRoutingNo));
                    Uow.AddInParameter(dbCommand, "VDebitorBankCode", DbType.String, 5, CheckWhiteSpaceAndSpecilChar(objInstr.DebitorBankCode));

                    Uow.AddInParameter(dbCommand, "VTOBICFI", DbType.String, 9, CheckWhiteSpaceAndSpecilChar(objInstr.TOBICFI));
                    Uow.AddInParameter(dbCommand, "VToRoutingNo", DbType.String, 9, CheckWhiteSpaceAndSpecilChar(objInstr.ToRoutingNo));
                    Uow.AddInParameter(dbCommand, "VCreditorBankCode", DbType.String, 5, CheckWhiteSpaceAndSpecilChar(objInstr.CreditorBankCode));

                    Uow.AddInParameter(dbCommand, "Vmserverdate", DbType.String, 12, CheckWhiteSpaceAndSpecilChar(objInstr.TrnDate));
                    Uow.AddInParameter(dbCommand, "VTtlIntrBkSttlmCurr", DbType.String, 20, CheckWhiteSpaceAndSpecilChar(objInstr.TtlIntrBkSttlmCurr));
                    Uow.AddInParameter(dbCommand, "VTtlIntrBkSttlmAmt", DbType.String, 20, objInstr.TtlIntrBkSttlmAmt);
                    Uow.AddInParameter(dbCommand, "Vmuser_code", DbType.String, 5, "101"); //CheckWhiteSpaceAndSpecilChar(objInstr.MuserCode));
                    Uow.AddInParameter(dbCommand, "VDbtrNm", DbType.String, 20, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrNm));
                    Uow.AddInParameter(dbCommand, "VDbtrAddress", DbType.String, 200, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAddress));
                    Uow.AddInParameter(dbCommand, "VDbtrStreet", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrStreetName));
                    Uow.AddInParameter(dbCommand, "VDbtrTown", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrTownName));
                    Uow.AddInParameter(dbCommand, "VDbtrCtry", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrCountry));
                    Uow.AddInParameter(dbCommand, "VDbtrAcctNo", DbType.String, 17, CheckWhiteSpaceAndSpecilChar(objInstr.DbtrAcctNo.Trim()));
                    Uow.AddInParameter(dbCommand, "VCdtrNm", DbType.String, 170, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrNm));

                    Uow.AddInParameter(dbCommand, "VCdtrAddress", DbType.String, 200, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrAddress));
                    Uow.AddInParameter(dbCommand, "VCdtrStreet", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrStreetName));
                    Uow.AddInParameter(dbCommand, "VCdtrTown", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrTownName));
                    Uow.AddInParameter(dbCommand, "VCdtrCtry", DbType.String, 150, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrCountry));

                    Uow.AddInParameter(dbCommand, "VCdtrAcctNo", DbType.String, 17, CheckWhiteSpaceAndSpecilChar(objInstr.CdtrAcctNo));
                    Uow.AddInParameter(dbCommand, "VRmtInfUstrd", DbType.String, 50, CheckWhiteSpaceAndSpecilChar(objInstr.RmtInfUstrdecononiccode));
                    Uow.AddInParameter(dbCommand, "VCheckNo", DbType.String, 7, CheckWhiteSpaceAndSpecilChar(objInstr.CheckNo));
                    Uow.AddInParameter(dbCommand, "VCheckDate", DbType.String, 12, CheckWhiteSpaceAndSpecilChar(objInstr.CheckDate));
                    Uow.AddInParameter(dbCommand, "VChargeApplyYN", DbType.String, 1, objInstr.ChargeApplyYN);
                    //------------VAT/TAX
                    Uow.AddInParameter(dbCommand, "VBIN_No", DbType.String, 50, objInstr.RmtInfUstrdBIN);
                    Uow.AddInParameter(dbCommand, "VEcononic_Code", DbType.String, 50, objInstr.RmtInfUstrdecononiccode);
                    Uow.AddInParameter(dbCommand, "VFloraBranchCode", DbType.String, 4, objInstr.FloraBranchCode);
                    //------------EndVAT/TAX
                    //------------CustomDuty
                    #region Custom Duty
                    Uow.AddInParameter(dbCommand, "VCustom_off_code", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.Custom_off_code));
                    Uow.AddInParameter(dbCommand, "VReg_year", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.Reg_year));
                    Uow.AddInParameter(dbCommand, "VReg_Num", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.Reg_Num));
                    Uow.AddInParameter(dbCommand, "VDec_code", DbType.String, 25, CheckWhiteSpaceAndSpecilChar(objInstr.Dec_code));
                    Uow.AddInParameter(dbCommand, "VCus_Mob_no", DbType.String, 25, objInstr.Cus_Mob_no);
                    #endregion Custom Duty
                    //------------EndCustomDuty
                    //Output
                    Uow.AddOutParameter(dbCommand, "Vmsg_code", DbType.String, 5, "");
                    Uow.AddOutParameter(dbCommand, "VMSG", DbType.String, 200, "");
                    Uow.AddOutParameter(dbCommand, "VoutBizMsgIdr", DbType.String, 35, "");
                    Uow.AddOutParameter(dbCommand, "VoutInstrId", DbType.String, 35, "");
                    await Uow.TblFiToFiCustomerCreditTranRepository.ExecuteStoredProc(dbCommand);
                    var cmd = (DbCommand)dbCommand;
                    messages[0] = cmd.Parameters["VMSG"].Value.ToString();
                    messages[1] = cmd.Parameters["Vmsg_code"].Value.ToString();
                    messages[2] = cmd.Parameters["VoutBizMsgIdr"].Value.ToString();
                    messages[3] = cmd.Parameters["VoutInstrId"].Value.ToString();
                }

                catch (Exception ex)
                {
                    throw ex;
                }
                return messages;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
