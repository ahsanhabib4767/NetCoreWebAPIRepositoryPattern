using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RTGSWebApi.Controllers;
using RTGSWebApi.Infrastructure.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RTGSWebApi.Model;
using RTGSWebApi.Transaction;
using Microsoft.AspNetCore.Authorization;
using RTGSWebApi.ViewModels.Rtgs;

namespace RTGSWebApi.Controllers
{   
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class RTGSController : BaseController
    {

        public RTGSController(ITransActionDP tDp, IOptions<ConfigurationOptions> options)
        {
            TDp = tDp;
            Options = options;
        }
        [Authorize]
        [Route("PaymentInstruction")]
        [HttpPost]
        //Ahsan_2020
        public async Task<dynamic> PaymentInstruction([FromBody] VatOnlineFiToFiCustomerCreditTran vatOnlineFiToFiCustomerCreditTran)
        {
            try
            {
                return await TDp.PaymentInstruction2(vatOnlineFiToFiCustomerCreditTran);
            }
            catch (FormatException fx)
            {
                var rslt = new
                {
                    MsgCode = "13",
                    Msg = "Duplicate message or Unique Id",
                    EdrTraceno = "",
                    OriginalTraceno = ""
                };
                //var reqData = JsonConvert.SerializeObject(postParam);
                //await TDp.ApiRequestLog();

                return rslt;
            }
            catch (Exception ex)
            {
                var rslt = new
                {
                    MsgCode = "Err",
                    Msg = "Invalid transaction",
                    EdrTraceno = "",
                    OriginalTraceno = ""
                };
                //var reqData = JsonConvert.SerializeObject(postParam);
                
                return rslt;
            }
        }

        //Reconcilation
        [Authorize]
        [Route("PaymentReconciliation")]
        [HttpPost]
        //Ahsan_2020
        public async Task<dynamic> PaymentReconciliation([FromBody] VatStatus vat)
        {
            try
            {
                return await TDp.PaymentReconciliation(vat);
            }
            catch (FormatException fx)
            {
                var rslt = new
                {
                    MsgCode = "13",
                    Msg = "Invalid Transaction",
                    EdrTraceno = "",
                    OriginalTraceno = ""
                }; 
                return rslt;
            }
            catch (Exception ex)
            {
                var rslt = new
                {
                    MsgCode = "Err",
                    Msg = "Invalid transaction",
                    EdrTraceno = "",
                    OriginalTraceno = ""
                };
                //var reqData = JsonConvert.SerializeObject(postParam);

                return rslt;
            }
        }
    }
}
