using RTGSWebApi.ViewModel.Rtgs;
using RTGSWebApi.ViewModels.Cbs;
using RTGSWebApi.ViewModels.Rtgs;
using System;
using System.Data.Common;
using System.Data.SqlClient;

namespace RTGSWebApi.Repository
{
    public class PopulateRecord : IPopulateRecord
    {
        #region PopulateRecord

        /// <summary>
        ///     Populate Record
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="entity"></param>
        /// <returns>result</returns>
        public virtual dynamic Populate(DbDataReader reader, string entity)
        {
            
            if (entity == "System36ViewModel")
            {
                return GetSystem36(reader);
            }
            if (entity == "VatOnlineInfo")
            {
                return GetVatOnlineInfo(reader);
            }
            if (entity == "FiToFiCustomerCreditTran")
            {
                return GetFiToFiCustomerCreditTran(reader);
            }
            if (entity == "AccEntity")
            {
                return GetAccEntity(reader);
            }
           
            if (entity == "ATypeCodeEntity")
            {
                return GetATypeCodeEntity(reader);
            }            

            return null;
        }

        public ATypeCodeEntity GetATypeCodeEntity(DbDataReader reader)
        {
            return new ATypeCodeEntity
            {
                ATypeCode = ColumnExists(reader, "ATypeCode") ? reader["ATypeCode"].ToString() : "",
                InwardOrOutward = ColumnExists(reader, "InwardOrOutward") ? reader["InwardOrOutward"].ToString() : "",
                SL = ColumnExists(reader, "SL") ? ((reader["SL"] == DBNull.Value) ? 0 : Convert.ToInt32(reader["SL"])) : 0
            };
        }

        
        private AccEntity GetAccEntity(DbDataReader reader)
        {
            return new AccEntity
            {
                ChrgAmt = Convert.ToDouble(reader["ChrargeAmt"]),
                IncAmt = Convert.ToDouble(reader["IncAmt"]),
                VatAmt = Convert.ToDouble(reader["VatAmt"]),
                HOBRCODE = reader["HOBRCODE"].ToString(),

                RtgsGLAccNo = reader["RTGS_glacno"].ToString(),
                RtgsIbtaAccNo = reader["RTGS_IBTA_acno"].ToString(),
                IncAccount = reader["IncAc"].ToString(),
                VatAccount = reader["VatAc"].ToString()

                //CBSTraceNO = reader["CbsTraceNo"] != System.DBNull.Value ? reader["CbsTraceNo"].ToString() : "",
                //ChargeApplyYN = reader["ChrgeApplyYN"].ToString(),
                //FromRoutingNo = reader["FromRoutingNo"].ToString(),
                //ToRoutingNo = reader["ToRoutingNo"].ToString(),
                //FloraBranchCode = reader["FloraBranchCode"].ToString(),

                //ChrgAmt = ColumnExists(reader, "ChrargeAmt") ? ((reader["ChrargeAmt"] == DBNull.Value) ? 0.00 : Convert.ToDouble(reader["ChrargeAmt"])) : 0.00,
                //IncAmt = ColumnExists(reader, "IncAmt") ? ((reader["IncAmt"] == DBNull.Value) ? 0.00 : Convert.ToDouble(reader["IncAmt"])) : 0.00,
                //VatAmt = ColumnExists(reader, "VatAmt") ? ((reader["VatAmt"] == DBNull.Value) ? 0.00 : Convert.ToDouble(reader["VatAmt"])) : 0.00,
                //HOBRCODE = ColumnExists(reader, "HOBRCODE") ? reader["HOBRCODE"].ToString() : "",

                //RtgsGLAccNo = ColumnExists(reader, "RTGS_glacno") ? reader["RTGS_glacno"].ToString() : "",
                //RtgsIbtaAccNo = ColumnExists(reader, "RTGS_IBTA_acno") ? reader["RTGS_IBTA_acno"].ToString() : "",
                //IncAccount = ColumnExists(reader, "IncAc") ? reader["IncAc"].ToString() : "",
                //VatAccount = ColumnExists(reader, "VatAc") ? reader["VatAc"].ToString() : ""
            };
        }

        private FiToFiCustomerCreditTran GetFiToFiCustomerCreditTran(DbDataReader reader)
        {
            return new FiToFiCustomerCreditTran
            {
                //objTrn.BizMsgIdr = reader["BizMsgIdr"].ToString();
                //objTrn.InstrId = reader["InstrId"].ToString();
                //objTrn.DbtrAcctNo = reader["DbtrAcctNo"].ToString();
                //objTrn.TtlIntrBkSttlmAmt = Convert.ToDouble(reader["IntrBkSttlmAmt"]);
                //objTrn.RmtInfUstrd = reader["RmtInfUstrd"].ToString();
                //objTrn.DebitorGLAcc = reader["DebitorGLAcc"] != System.DBNull.Value ? reader["DebitorGLAcc"].ToString() : "";

                //objTrn.TransectionType = reader["LclPrtry"] != System.DBNull.Value ? reader["LclPrtry"].ToString() : "";
                //objTrn.CheckNo = reader["CheckNo"] != System.DBNull.Value ? reader["CheckNo"].ToString() : "";
                //objTrn.CheckDate = reader["CheckDate"] != System.DBNull.Value ? Convert.ToDateTime(reader["CheckDate"].ToString()).ToShortDateString() : "";
                ////objTrn.AllowedInwordATypeCode = reader["AllowedInwordATypeCode"] != System.DBNull.Value ? reader["AllowedInwordATypeCode"].ToString() : "";
                //objTrn.AllowedOutwardATypeCode = reader["AllowedOutwardATypeCode"] != System.DBNull.Value ? reader["AllowedOutwardATypeCode"].ToString() : "";

                //#region T24
                //objTrn.TtlIntrBkSttlmCurr = reader["IntrBkSttlmAmtCcy"].ToString();
                //objTrn.CdtrAcctNo = reader["CdtrAcctNo"].ToString();
                //objTrn.CdtrNm = reader["CdtrNm"].ToString();
                //objTrn.FromRoutingNo = reader["DbtrAgtFIBICFI"].ToString();//use for payee bank narration

                ObjAccounting = GetAccEntity(reader),

                BizMsgIdr = ColumnExists(reader, "BizMsgIdr") ? reader["BizMsgIdr"].ToString() : "",
                InstrId = ColumnExists(reader, "InstrId") ? reader["InstrId"].ToString() : "",
                DbtrAcctNo = ColumnExists(reader, "DbtrAcctNo") ? reader["DbtrAcctNo"].ToString() : "",
                TtlIntrBkSttlmAmt = ColumnExists(reader, "IntrBkSttlmAmt") ? ((reader["IntrBkSttlmAmt"] == DBNull.Value) ? 0.00 : Convert.ToDouble(reader["IntrBkSttlmAmt"])) : 0.00,

                RmtInfUstrd = ColumnExists(reader, "RmtInfUstrd") ? reader["RmtInfUstrd"].ToString() : "",
                DebitorGLAcc = ColumnExists(reader, "DebitorGLAcc") ? reader["DebitorGLAcc"].ToString() : "",

                TransectionType = ColumnExists(reader, "LclPrtry") ? reader["LclPrtry"].ToString() : "",
                CheckNo = ColumnExists(reader, "CheckNo") ? reader["CheckNo"].ToString() : "",
                CheckDate = reader["CheckDate"] != System.DBNull.Value ? Convert.ToDateTime(reader["CheckDate"].ToString()).ToShortDateString() : "",

                AllowedInwordATypeCode = ColumnExists(reader, "AllowedInwordATypeCode") ? reader["AllowedInwordATypeCode"].ToString() : "",
                TtlIntrBkSttlmCurr = ColumnExists(reader, "IntrBkSttlmAmtCcy") ? reader["IntrBkSttlmAmtCcy"].ToString() : "",

                CdtrAcctNo = ColumnExists(reader, "CdtrAcctNo") ? reader["CdtrAcctNo"].ToString() : "",
                CdtrNm = ColumnExists(reader, "CdtrNm") ? reader["CdtrNm"].ToString() : "",
                FromRoutingNo = ColumnExists(reader, "DbtrAgtFIBICFI") ? reader["DbtrAgtFIBICFI"].ToString() : "",

                CBSTraceNO = ColumnExists(reader, "CbsTraceNo") ? reader["CbsTraceNo"].ToString() : "",
                ChargeApplyYN = ColumnExists(reader, "ChrgeApplyYN") ? reader["ChrgeApplyYN"].ToString() : "",
                //FromRoutingNo = ColumnExists(reader, "FromRoutingNo") ? reader["FromRoutingNo"].ToString() : "",
                ToRoutingNo = ColumnExists(reader, "ToRoutingNo") ? reader["ToRoutingNo"].ToString() : "",
                FloraBranchCode = ColumnExists(reader, "FloraBranchCode") ? reader["FloraBranchCode"].ToString() : ""

                //objTrn.CBSTraceNO = reader["CbsTraceNo"] != System.DBNull.Value ? reader["CbsTraceNo"].ToString() : "";
                //objTrn.ChargeApplyYN = reader["ChrgeApplyYN"].ToString();
                //objTrn.FromRoutingNo = reader["FromRoutingNo"].ToString();
                //objTrn.ToRoutingNo = reader["ToRoutingNo"].ToString();
                //objTrn.FloraBranchCode = reader["FloraBranchCode"].ToString();
            };
        }

        private VatOnlineInfo GetVatOnlineInfo(DbDataReader reader)
        {
            return new VatOnlineInfo
            {
                PaymentRef = ColumnExists(reader, "PaymentRef") ? reader["PaymentRef"].ToString() : "",
                Bin = ColumnExists(reader, "Bin") ? reader["Bin"].ToString() : "",
                EconomicCode = ColumnExists(reader, "EconomicCode") ? reader["EconomicCode"].ToString() : "",
                Amount = ColumnExists(reader, "Amount") ? ((reader["Amount"] == DBNull.Value) ? 0.00 : Convert.ToDouble(reader["Amount"])) : 0.00,
                Echallan = ColumnExists(reader, "Echallan") ? reader["Echallan"].ToString() : "",
                EchallanDate = reader["EchallanDate"] != System.DBNull.Value ? Convert.ToDateTime(reader["EchallanDate"].ToString()).ToShortDateString() : "",
                Status = ColumnExists(reader, "Status") ? reader["Status"].ToString() : ""
            };
        }

        public System36ViewModel GetSystem36(DbDataReader reader)
        {
            return new System36ViewModel
            {
                System36 = ColumnExists(reader, "system36") ? reader["system36"].ToString() : "",
                System36Net = ColumnExists(reader, "system36_net") ? reader["system36_net"].ToString() : "",
                App36Net = ColumnExists(reader, "app36_net") ? reader["app36_net"].ToString() : "",
                ReadWriteUser = ColumnExists(reader, "readWrite_User") ? reader["readWrite_User"].ToString() : "",
                DbType = ColumnExists(reader, "DBType") ? reader["DBType"].ToString() : ""
            };
        }

        #endregion

        #region Column Exists

        /// <summary>
        ///     ColumnExists
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="columnName"></param>
        /// <returns>boolean value</returns>
        public bool ColumnExists(DbDataReader reader, string columnName)
        {
            for (var i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public dynamic Populate(SqlDataReader reader, string entity)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}