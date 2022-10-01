using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace RTGSWebApi.Repository
{
    public interface IRepository<T> where T : class
    {
        #region Execute Inline Query

        /// <summary>
        ///     Get All Execute Text
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAllExecuteText(string strText);

        /// <summary>
        ///     Get By ID Execute Text
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        Task<T> GetByIdExecuteText(string strText);

        /// <summary>
        ///     Execute Text
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        Task<int> ExecuteText(string strText);

        /// <summary>
        /// ExecuteText
        /// </summary>
        /// <param name="strText"></param>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        Task<int> ExecuteText(string strText, DbTransaction dbTransaction);

        /// <summary>
        ///     Get Result By Execute Text
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        Task<dynamic> GetResultByExecuteText(string strText);

        /// <summary>
        ///     Get Single Value By Execute Text
        /// </summary>
        /// <param name="strText"></param>
        /// <returns></returns>
        Task<dynamic> GetSingleValueByExecuteText(string strText);

        #endregion

        #region Execute Stored Procedure

        /// <summary>
        ///     Execute Stored Procedure
        /// </summary>
        /// <param name="dbCommand"></param>
        /// <returns></returns>
        /// Execute non query Use for Data Definition tasks(Creating SP and view) as well as Data Manipulation(Insert,delete,update)-Ahsan
        Task<dynamic> ExecuteStoredProc(IDbCommand dbCommand);

        /// <summary>
        /// Execute Stored Procedure (With Transaction)
        /// </summary>
        /// <param name="dbcmd"></param>
        /// <param name="transaction"></param>
        /// <returns>result</returns>
        Task<dynamic> ExecuteStoredProc(IDbCommand dbcmd, DbTransaction transaction);

        /// <summary>
        ///     Get By Execute Stored Procedure
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        Task<T> GetByExecuteStoredProc(IDbCommand cmd);

        /// <summary>
        /// Get By Execute Stored Procedure
        /// </summary>
        /// <param name="dbcmd"></param>
        /// <param name="transaction"></param>
        /// <returns></returns>
        Task<T> GetByExecuteStoredProc(IDbCommand dbcmd, DbTransaction transaction);

        /// <summary>
        ///     Get All Execute Stored Procedure
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        Task<IEnumerable<T>> GetAllExecuteStoredProc(IDbCommand cmd);

        /// <summary>
        ///     Get Single Value By Stored Procedure
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        Task<dynamic> GetSingleValueByStoredProc(IDbCommand cmd);

        /// <summary>
        ///     Get Result By Execute Stored Procedure
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        Task<Tuple<string, int, bool, dynamic>> GetResultByExecuteStoredProc(DbCommand cmd);

        #endregion
    }
}