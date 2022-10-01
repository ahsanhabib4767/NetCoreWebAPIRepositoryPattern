using RTGSWebApi.Infrastructure.Common;
using RTGSWebApi.Infrastructure.Utility;
using RTGSWebApi.Repository;
using RTGSWebApi.ViewModels;
using RTGSWebApi.ViewModels.Rtgs;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using RTGSWebApi.ViewModel.Rtgs;
using RTGSWebApi.Model;

namespace RTGSWebApi.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly SqlConnection _context;
        private bool _disposed;

        public UnitOfWork(IOptions<ConfigurationOptions> conOption)
        {
            string con, userid, pass;
            var rtgsDbServerIpOrName = conOption.Value.RtgsDbServerIpOrName;
            var rtgsDbName = conOption.Value.RtgsDbName;

            //var securityConn = "Server=" + rtgsDbServerIpOrName + ";database=" + rtgsDbName + ";
            //var system36View = GetAllExecuteText("select * from dbo.system36 where [DBType]='R'", securityConn);
            //if (system36View.Any())
            //{
            //    pass = GetPassword(system36View.First().System36Net).Result;
            //    userid = GetUser(system36View.First().ReadWriteUser).Result;
            //}
            //else
            //{
            //    throw new Exception("System36 Not Configured!");
            //}
            con = "Server=" + rtgsDbServerIpOrName + ";database=" + rtgsDbName+ ";Trusted_Connection=true;MultipleActiveResultSets=true";
            _context = new SqlConnection(con);
        }        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }       

        private GenericRepository<VatOnlineInfo> _tblVatOnlineInfoRepository;
        public IRepository<VatOnlineInfo> TblVatOnlineInfoRepository
        {
            get
            {
                if (_tblVatOnlineInfoRepository == null)
                    _tblVatOnlineInfoRepository = new GenericRepository<VatOnlineInfo>(_context);
                return _tblVatOnlineInfoRepository;
            }
        }

        private GenericRepository<FiToFiCustomerCreditTran> _tblFiToFiCustomerCreditTranRepository;
        public IRepository<FiToFiCustomerCreditTran> TblFiToFiCustomerCreditTranRepository
        {
            get
            {
                if (_tblFiToFiCustomerCreditTranRepository == null)
                    _tblFiToFiCustomerCreditTranRepository = new GenericRepository<FiToFiCustomerCreditTran>(_context);
                return _tblFiToFiCustomerCreditTranRepository;
            }
        }

        private GenericRepository<System36ViewModel> _tblSystem36Repository;
        public IRepository<System36ViewModel> TblSystem36Repository
        {
            get
            {
                if (_tblSystem36Repository == null)
                    _tblSystem36Repository = new GenericRepository<System36ViewModel>(_context);
                return _tblSystem36Repository;
            }
        }

        private GenericRepository<ATypeCodeEntity> _tblATypeCodeEntityRepository;
        public IRepository<ATypeCodeEntity> TblATypeCodeEntityRepository
        {
            get
            {
                if (_tblATypeCodeEntityRepository == null)
                    _tblATypeCodeEntityRepository = new GenericRepository<ATypeCodeEntity>(_context);
                return _tblATypeCodeEntityRepository;
            }
        }

        private GenericRepository<LoginModel> _tblLoginModelRepository;
        public IRepository<LoginModel> TblLoginModelRepository
        {
            get
            {
                if (_tblLoginModelRepository == null)
                    _tblLoginModelRepository = new GenericRepository<LoginModel>(_context);
                return _tblLoginModelRepository;
            }
        }

        public async Task<DbTransaction> SetDbTransaction()
        {
            var dbCon = _context;
            if (dbCon.State == ConnectionState.Broken || dbCon.State == ConnectionState.Closed)
            {
                dbCon.Open();
            }
            return await Task.FromResult(dbCon.BeginTransaction());
        }

        /// <summary>
        ///     Db Stored Procedure
        /// </summary>
        /// <param name="storedProcedure"></param>
        /// <returns>result</returns>
        public IDbCommand DbStoredProcedure(string storedProcedure)
        {
            var dbCon = _context;
            IDbCommand dbCommand = dbCon.CreateCommand();
            dbCommand.CommandTimeout = 0;
            dbCommand.CommandText = storedProcedure;
            dbCommand.CommandType = CommandType.StoredProcedure;
            return dbCommand;
        }

        /// <summary>
        ///     Add In Parameter
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="strParamName"></param>
        /// <param name="dbType"></param>
        /// <param name="dbSize"></param>
        /// <param name="strParamValue"></param>
        /// <returns>result</returns>
        public IDbCommand AddInParameter(IDbCommand dbCommand, string strParamName, DbType dbType, int dbSize,
            dynamic strParamValue)
        {
            var dbparam = dbCommand.CreateParameter();
            dbparam.ParameterName = strParamName;
            dbparam.DbType = dbType;
            dbparam.Value = strParamValue;
            dbparam.Size = dbSize;
            dbCommand.Parameters.Add(dbparam);
            return dbCommand;
        }

        /// <summary>
        ///     Add Out Parameter
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <param name="strParamName"></param>
        /// <param name="dbType"></param>
        /// <param name="dbSize"></param>
        /// <param name="strParamValue"></param>
        /// <returns>result</returns>
        public IDbCommand AddOutParameter(IDbCommand dbCommand, string strParamName, DbType dbType, int dbSize,
            dynamic strParamValue)
        {
            var dbparam = dbCommand.CreateParameter();
            dbparam.ParameterName = strParamName;
            dbparam.DbType = dbType;
            dbparam.Size = dbSize;
            dbparam.Direction = ParameterDirection.Output;
            dbCommand.Parameters.Add(dbparam);
            return dbCommand;
        }

        public async Task<string> GetPassword(string pass)
        {
            try
            {
                var isEncrypted = SystemSecurity.IsEncrypted(pass);
                if (isEncrypted)
                {
                    var decryptedPassword = SystemSecurity.Decrypt(pass);
                    pass = decryptedPassword.Substring(6);
                }
                return await Task.FromResult(pass);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<string> GetUser(string user)
        {
            try
            {
                var isEncrypted = SystemSecurity.IsEncrypted(user);
                if (isEncrypted)
                {
                    var decryptedUser = SystemSecurity.Decrypt(user);
                    user = decryptedUser.Substring(6);
                }
                return await Task.FromResult(user);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<System36ViewModel> GetAllExecuteText(string strText, string strCon)
        {
            var dbConnection = new SqlConnection(strCon);
            IPopulateRecord populateRecord = new PopulateRecord();
            var list = new List<System36ViewModel>();
            try
            {
                if (dbConnection.State == ConnectionState.Broken || dbConnection.State == ConnectionState.Closed)
                {
                    dbConnection.Open();
                }
                DbCommand cmd = dbConnection.CreateCommand();
                cmd.CommandText = strText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = dbConnection;
                var reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        list.Add(populateRecord.Populate(reader, "System36ViewModel"));
                    }
                }
                finally
                {
                    reader.Dispose();
                }
            }
            finally
            {
                dbConnection.Close();
            }
            return list;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }
    }
}