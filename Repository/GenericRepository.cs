using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace RTGSWebApi.Repository
{
    public class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        #region Parameterized constructor

        /// <summary>
        ///     Generic Repository
        /// </summary>
        /// <param name="dbConnection"></param>
        public GenericRepository(SqlConnection dbConnection)
        {
            _dConnection = dbConnection;
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

        #endregion

        #region Private properties

        private readonly SqlConnection _dConnection;
        private readonly IPopulateRecord _populateRecord = new PopulateRecord();

        #endregion;

        #region Execute Inline Query

        /// <summary>
        ///     Get All ExecuteText
        /// </summary>
        /// <param name="strText"> sql Query</param>
        /// <returns> result </returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllExecuteText(string strText)
        {
            var list = new List<TEntity>();
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                DbCommand cmd = _dConnection.CreateCommand();
                cmd.CommandText = strText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _dConnection;
                var reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        //list.Add(PopulateRecord(reader));
                        list.Add(_populateRecord.Populate(reader, typeof (TEntity).Name));
                    }
                }
                finally
                {
                    reader.Dispose();
                    //((IDataReader)reader).Close();
                }
            }
            finally
            {
                _dConnection.Close();
            }
            return await Task.FromResult(list);
        }

        /// <summary>
        ///     Get By Id Execute Text
        /// </summary>
        /// <param name="strText">sql query</param>
        /// <returns>result</returns>
        public virtual async Task<TEntity> GetByIdExecuteText(string strText)
        {
            TEntity record = null;
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                DbCommand cmd = _dConnection.CreateCommand();
                cmd.CommandText = strText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _dConnection;
                var reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        //record = PopulateRecord(reader);
                        record = _populateRecord.Populate(reader, typeof (TEntity).Name);
                        break;
                    }
                }
                finally
                {
                    reader.Dispose();
                }
            }
            finally
            {
                _dConnection.Close();
            }
            return await Task.FromResult(record);
        }

        /// <summary>
        ///     ExecuteText (Only execute)
        /// </summary>
        /// <param name="strText">sql query</param>
        /// <returns>result</returns>
        public virtual async Task<int> ExecuteText(string strText)
        {
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                DbCommand cmd = _dConnection.CreateCommand();
                cmd.CommandText = strText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _dConnection;
                return await Task.FromResult(cmd.ExecuteNonQuery());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dConnection.Close();
            }
        }

        /// <summary>
        ///     ExecuteText (Only execute)
        /// </summary>
        /// <param name="strText">sql query</param>
        /// <param name="dbTransaction"></param>
        /// <returns>result</returns>
        public virtual async Task<int> ExecuteText(string strText, DbTransaction dbTransaction)
        {
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                DbCommand cmd = _dConnection.CreateCommand();
                cmd.CommandText = strText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _dConnection;
                cmd.Transaction = dbTransaction;
                return await Task.FromResult(cmd.ExecuteNonQuery());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dConnection.Close();
            }
        }

        /// <summary>
        ///     Get Result By ExecuteText
        /// </summary>
        /// <param name="strText">sql query</param>
        /// <returns>result</returns>
        public virtual async Task<dynamic> GetResultByExecuteText(string strText)
        {
            dynamic record = null;
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                DbCommand cmd = _dConnection.CreateCommand();
                cmd.CommandText = strText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _dConnection;
                var reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        record = (reader[0] == DBNull.Value) ? 0 : reader[0];
                    }
                }
                finally
                {
                    reader.Dispose();
                }
            }
            finally
            {
                _dConnection.Close();
            }
            if (record == null)
            {
                return await Task.FromResult("");
            }
            return await Task.FromResult(record);
        }

        /// <summary>
        ///     Get Single Value By Execute Text
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        public virtual async Task<dynamic> GetSingleValueByExecuteText(string strText)
        {
            dynamic record = null;
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                DbCommand cmd = _dConnection.CreateCommand();
                cmd.CommandText = strText;
                cmd.CommandType = CommandType.Text;
                cmd.Connection = _dConnection;
                record = cmd.ExecuteScalar();
            }
            finally
            {
                _dConnection.Close();
            }
            if (record == null)
            {
                return await Task.FromResult("");
            }
            return await Task.FromResult(record);
        }

        #endregion  Execute Inline Query

        #region Execute Stored Procedure 

        /// <summary>
        ///     Execute Stored Procedure
        /// </summary>
        /// <param name="dbcmd"> sql command</param>
        /// <returns>result</returns>
        public virtual async Task<dynamic> ExecuteStoredProc(IDbCommand dbcmd)
        {
            var cmd = (DbCommand) dbcmd;
            //var listMsg = new List<string>();
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                cmd.Connection = _dConnection;
                cmd.CommandType = CommandType.StoredProcedure;

                return await Task.FromResult(cmd.ExecuteNonQuery());
                //listMsg.Add((string) cmd.Parameters["VMSG_CODE"].Value);
                //listMsg.Add((string) cmd.Parameters["VMSG"].Value);
                //return listMsg;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dConnection.Close();
            }
        }

        /// <summary>
        ///     Execute Stored Procedure (With Transaction)
        /// </summary>
        /// <param name="dbcmd"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public virtual async Task<dynamic> ExecuteStoredProc(IDbCommand dbcmd, DbTransaction transaction)
        {
            var cmd = (DbCommand) dbcmd;
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                cmd.Connection = _dConnection;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Transaction = transaction;
                return await Task.FromResult(cmd.ExecuteNonQuery());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dConnection.Close();
            }
        }

        /// <summary>
        ///     Get By Execute Stored Procedure
        /// </summary>
        /// <param name="dbcmd">sql command</param>
        /// <returns>result TEntity</returns>
        public virtual async Task<TEntity> GetByExecuteStoredProc(IDbCommand dbcmd)
        {
            TEntity record = null;
            var cmd = (DbCommand) dbcmd;
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                cmd.Connection = _dConnection;
                var reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        record = _populateRecord.Populate(reader, typeof (TEntity).Name);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dConnection.Close();
            }
            return await Task.FromResult(record);
        }

        /// <summary>
        ///     Get By Execute Stored Procedure
        /// </summary>
        /// <param name="dbcmd">sql command</param>
        /// <returns>result TEntity</returns>
        public virtual async Task<TEntity> GetByExecuteStoredProc(IDbCommand dbcmd, DbTransaction transaction)
        {
            TEntity record = null;
            var cmd = (DbCommand) dbcmd;
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                cmd.Connection = _dConnection;
                cmd.Transaction = transaction;
                var reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        record = _populateRecord.Populate(reader, typeof (TEntity).Name);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dConnection.Close();
            }
            return await Task.FromResult(record);
        }

        /// <summary>
        ///     Get All Execute Stored Procedure
        /// </summary>
        /// <param name="dbcmd">sql command</param>
        /// <returns>result TEntity list</returns>
        public virtual async Task<IEnumerable<TEntity>> GetAllExecuteStoredProc(IDbCommand dbcmd)
        {
            var cmd = (DbCommand) dbcmd;
            var list = new List<TEntity>();
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                cmd.Connection = _dConnection;
                var reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        list.Add(_populateRecord.Populate(reader, typeof (TEntity).Name));
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dConnection.Close();
            }
            return await Task.FromResult(list);
        }

        /// <summary>
        ///     Get Single Value By Stored Procedure
        /// </summary>
        /// <param name="dbcmd"> sql command</param>
        /// <returns>Single Value</returns>
        public virtual async Task<dynamic> GetSingleValueByStoredProc(IDbCommand dbcmd)
        {
            var cmd = (DbCommand) dbcmd;
            dynamic record = null;
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                cmd.Connection = _dConnection;
                record = cmd.ExecuteScalar();
            }
            finally
            {
                _dConnection.Close();
            }
            return await Task.FromResult(record);
        }

        /// <summary>
        ///     Get Result ByExecute Stored Procedure
        /// </summary>
        /// <param name="cmd">sql command</param>
        /// <returns> result tuple</returns>
        public virtual async Task<Tuple<string, int, bool, dynamic>> GetResultByExecuteStoredProc(DbCommand cmd)
        {
            dynamic record = null;
            string retMsg = null;
            var i = 0;
            var bl = false;
            try
            {
                if (_dConnection.State == ConnectionState.Broken || _dConnection.State == ConnectionState.Closed)
                {
                    _dConnection.Open();
                }
                cmd.Connection = _dConnection;
                var reader = cmd.ExecuteReader();
                retMsg = (string) cmd.Parameters["@Msg"].Value;
                try
                {
                    while (reader.Read())
                    {
                        record = _populateRecord.Populate(reader, typeof (TEntity).Name);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    reader.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                _dConnection.Close();
            }
            return await Task.FromResult(Tuple.Create<string, int, bool, dynamic>(retMsg, i, bl, record));
        }

        #endregion Execute Stored Procedure

        #region Data Reader Map

        /// <summary>
        ///     Data Reader Map To List
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns>result</returns>
        private List<T> DataReaderMapToList<T>(DbDataReader dr)
        {
            var list = new List<T>();
            var obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                var fieldCount = dr.FieldCount;
                foreach (var prop in obj.GetType().GetProperties())
                {
                    if (ColumnExists(dr, prop.Name))
                    {
                        if (!Equals(dr[prop.Name], DBNull.Value))
                        {
                            prop.SetValue(obj, dr[prop.Name], null);
                        }
                    }
                    else
                    {
                        prop.SetValue(obj, null, null);
                    }
                }
                list.Add(obj);
            }

            return list;
        }

        /// <summary>
        ///     Data Reader Map To Entity
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns>return entity</returns>
        public T DataReaderMapToEntity<T>(DbDataReader dr)
        {
            var obj = default(T);
            while (dr.Read())
            {
                obj = Activator.CreateInstance<T>();
                var fieldCount = dr.FieldCount;
                foreach (var prop in obj.GetType().GetProperties())
                {
                    if (ColumnExists(dr, prop.Name))
                    {
                        if (!Equals(dr[prop.Name], DBNull.Value))
                        {
                            prop.SetValue(obj, dr[prop.Name], null);
                        }
                    }
                    else
                    {
                        prop.SetValue(obj, null, null);
                    }
                }
            }

            return obj;
        }

        #endregion
    }
}