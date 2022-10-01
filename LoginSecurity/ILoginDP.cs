using RTGSWebApi.ViewModels.Rtgs;
using RTGSWebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RTGSWebApi.LoginSecurity
{
    public interface ILoginDP
    {
        Task<string> CheckCredential(string userCode, string password);

    }
}
