using RTGSWebApi.Repository;
using RTGSWebApi.ViewModels.Rtgs;
using RTGSWebApi.ViewModel.Rtgs;

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using RTGSWebApi.Model;

namespace RTGSWebApi.UnitOfWorks
{
    public interface IUnitOfWork
    {
        IRepository<VatOnlineInfo> TblVatOnlineInfoRepository { get; }
        IRepository<FiToFiCustomerCreditTran> TblFiToFiCustomerCreditTranRepository { get; }       
        IRepository<System36ViewModel> TblSystem36Repository { get; }
        IRepository<ATypeCodeEntity> TblATypeCodeEntityRepository { get; }
        IRepository<LoginModel> TblLoginModelRepository { get; }

        Task<DbTransaction> SetDbTransaction();
        IDbCommand DbStoredProcedure(string strSpName);
        IDbCommand AddInParameter(IDbCommand dbCommand, string strParamName, DbType strDbType, int dbSize,dynamic strParamValue);
        IDbCommand AddOutParameter(IDbCommand dbCommand, string strParamName, DbType strDbType, int dbSize,dynamic strParamValue);
        Task<string> GetPassword(string pass);
        IEnumerable<System36ViewModel> GetAllExecuteText(string strText, string strCon);
    }
}