using RTGSWebApi.Infrastructure.Common;
using RTGSWebApi.Infrastructure.Utility;
using RTGSWebApi.Repository;
using RTGSWebApi.ViewModels.Cbs;
using RTGSWebApi.ViewModels.Rtgs;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace RTGSWebApi.UnitOfWorks
{
    public class UnitOfWorkCbs : IUnitOfWorkCbs, IDisposable
    {
        private readonly SqlConnection _context;
        private bool _disposed;

        public UnitOfWorkCbs(IOptions<ConfigurationOptions> conOption)
        {
            string con, userid, pass;
            var rtgsDbServerIpOrName = conOption.Value.RtgsDbServerIpOrName;
            var rtgsDbName = conOption.Value.RtgsDbName;
            var cbsDbName = conOption.Value.CbsDbName;


            //var securityConn = "Server=" + rtgsDbServerIpOrName + ";database=" + rtgsDbName + ";uid=system36;pwd=sys36";
            //var system36View = GetAllExecuteText("select * from dbo.system36 where [DBType]='C'", securityConn);
            //if (system36View.Any())
            //{
            //    pass = GetPassword(system36View.First().System36Net).Result;
            //    userid = GetUser(system36View.First().ReadWriteUser).Result;
            //}
            //else
            //{
            //    throw new Exception("System36 Not Configured!");
            //}
            //var cbsDbServerIpOrName = conOption.Value.CbsDbServerIpOrName;
            //var cbsDbName = conOption.Value.CbsDbName;
            //con = "Server=" + cbsDbServerIpOrName + ";database=" + cbsDbName + ";uid=" + userid + ";pwd=" + pass;
            //string rtgsConn = "Server=" + rtgsDbServerIpOrName + ";database=" + rtgsDbName + ";pooling=false;uid=system36;pwd=sys36";

            con = "Server=" + rtgsDbServerIpOrName + ";database=" + rtgsDbName;
            //var dataTable = GetAllExecuteText("select * from Parameter_Cridential where [id]='1'", con);

            //if (dataTable.Any())
            //{
            //    pass = GetPassword(dataTable.First().System36Net).Result;
            //    userid = GetUser(dataTable.First().ReadWriteUser).Result;
            //}
            //else
            //{
            //    throw new Exception("System36 Not Configured!");
            //}

            //con = "Server=" + rtgsDbServerIpOrName + ";database=" + cbsDbName + ";pooling=false;uid=" + userid + ";pwd=" + pass;
            _context = new SqlConnection(con);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private GenericRepository<CbsCustomer> _tblCbsCustomerRepository;
        public IRepository<CbsCustomer> TblCbsCustomerRepository
        {
            get
            {
                if (_tblCbsCustomerRepository == null)
                    _tblCbsCustomerRepository = new GenericRepository<CbsCustomer>(_context);
                return _tblCbsCustomerRepository;
            }
        }

        private GenericRepository<CbsMsg> _tblCbsMsgRepository;
        public IRepository<CbsMsg> TblCbsMsgRepository
        {
            get
            {
                if (_tblCbsMsgRepository == null)
                    _tblCbsMsgRepository = new GenericRepository<CbsMsg>(_context);
                return _tblCbsMsgRepository;
            }
        }

        private GenericRepository<AccEntity> _tblAccEntityRepository;
        public IRepository<AccEntity> TblAccEntityRepository
        {
            get
            {
                if (_tblAccEntityRepository == null)
                    _tblAccEntityRepository = new GenericRepository<AccEntity>(_context);
                return _tblAccEntityRepository;
            }
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