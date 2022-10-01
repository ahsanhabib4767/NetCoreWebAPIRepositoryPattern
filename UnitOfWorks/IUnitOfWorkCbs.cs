using RTGSWebApi.Repository;
using RTGSWebApi.ViewModels.Cbs;
using RTGSWebApi.ViewModels.Rtgs;
using System.Collections.Generic;
using System.Data;

namespace RTGSWebApi.UnitOfWorks
{
    public interface IUnitOfWorkCbs
    {
        IRepository<CbsCustomer> TblCbsCustomerRepository { get; }
        IRepository<CbsMsg> TblCbsMsgRepository { get; }
        IRepository<AccEntity> TblAccEntityRepository { get; }
       
        IDbCommand DbStoredProcedure(string strSpName);
        IDbCommand AddInParameter(IDbCommand dbCommand, string strParamName, DbType strDbType, int dbSize, dynamic strParamValue);
        IDbCommand AddOutParameter(IDbCommand dbCommand, string strParamName, DbType strDbType, int dbSize, dynamic strParamValue);
        IEnumerable<System36ViewModel> GetAllExecuteText(string strText, string strCon);

    }
}