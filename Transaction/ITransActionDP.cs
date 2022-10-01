using RTGSWebApi.ViewModels.Rtgs;
using RTGSWebApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RTGSWebApi.Transaction
{
    public interface ITransActionDP
    {
        Task<dynamic> PaymentReconciliation(VatStatus entity);
        Task<dynamic> PaymentInstruction(VatOnlineFiToFiCustomerCreditTran entity);
        Task<dynamic> PaymentInstruction2(VatOnlineFiToFiCustomerCreditTran entity);
        //Task<dynamic> FiCustomerCreditTransfer(FiToFiCustomerCreditTran entity);


    }
}
