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
using RTGSWebApi.Transaction;

namespace RTGSWebApi.LoginSecurity
{
    public class Login : BaseController, ILoginDP
    {
        public Login(IUnitOfWork uow, IOptions<ConfigurationOptions> options)
        {
           
            Uow = uow;
            //UowCbs = uowcbs;
            Options = options;
        }
        //Ahsan_2020
        public async Task<string> CheckCredential(string userCode, string password)
        {
            string selectQuery = "select UserName,UserPass from Parameter_Cridential  where UserName='" + userCode + "' and UserPass='" + password + "' ";
            try
            {
                var res =  await Uow.TblLoginModelRepository.GetResultByExecuteText(selectQuery);
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

    }
}
