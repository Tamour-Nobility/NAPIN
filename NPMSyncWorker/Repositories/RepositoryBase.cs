using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPMSyncWorker.Repositories
{
    internal abstract class RepositoryBase
    {
        protected IDbTransaction Transaction { get; private set; }
        protected IDbConnection Connection { get { return Transaction.Connection; } }

        public RepositoryBase(IDbTransaction transaction)
        {
            Transaction = transaction;
        }

        /// <summary>
        /// Add or update and entity
        /// </summary>
        /// <param name="procedure"></param>
        /// <param name="parameters"></param>
        /// <returns>Number of affected rows</returns>
        public int AddOrUpdate<TEntity>(string procedure, object parameters) where TEntity : class
        {
            return Connection.Execute(
                procedure,
                param: parameters,
                Transaction,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// Get entity by id
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="procedure"></param>
        /// <param name="id"></param>
        /// <returns>Entitys</returns>
        public TEntity Get<TEntity>(string procedure, long id) where TEntity : class
        {
            return Connection.QueryFirstOrDefault<TEntity>(
                procedure,
                param: new { pId = id },
                Transaction,
                commandType: CommandType.StoredProcedure
             );
        }

        /// <summary>
        /// Get list of entitiess
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="procedure"></param>
        /// <returns>List of Entities</returns>
        public List<TEntity> Get<TEntity>(string procedure) where TEntity : class
        {
            return Connection.Query<TEntity>(
                procedure,
                param: null,
                Transaction,
                commandType: CommandType.StoredProcedure
            ).AsList();
        }
    }
}
