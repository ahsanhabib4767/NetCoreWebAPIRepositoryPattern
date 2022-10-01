using System.Data.Common;

namespace RTGSWebApi.Repository
{
    public interface IPopulateRecord
    {
        /// <summary>
        /// Populate
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        dynamic Populate(DbDataReader reader, string entity);
    }
}
