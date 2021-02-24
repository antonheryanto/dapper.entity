using System;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dapper.Entity
{
    public partial class Database : DbContext
    {
        readonly bool _lowerCaseTable = false;
        Func<DatabaseFacade, DbConnection> _connectionFn = (db) => db.GetDbConnection();
        public Database(DbContextOptions options, 
            Func<DbConnection, DbConnection> dbWrapper = null,
            bool lowerCaseTable = false) : base(options)
        {
            _lowerCaseTable = lowerCaseTable;
            if (dbWrapper != null)
                _connectionFn = (db) => dbWrapper(Database.GetDbConnection());
        }

        public Database(DbContextOptions options) : base(options) {}

        public DatabaseType DatabaseType => Database.ProviderName switch
        {            
            "SqlServer" => DatabaseType.SqlServer,
            _ => DatabaseType.MySql,
        };

        DbConnection Cn() => _connectionFn(Database);

        DbTransaction Tx() => Database.CurrentTransaction?.GetDbTransaction();

        public async Task<List<dynamic>> QueryAsync(string sql, object param = null)
            => (await Cn().QueryAsync(sql, param, Tx())).AsList();

        public async Task<List<T>> QueryAsync<T>(
             string sql, object param = null)
            => (await Cn().QueryAsync<T>(sql, param, Tx())).AsList();

        public async Task<T> QueryFirstOrDefaultAsync<T>(
             string sql, object param = null)
            => (await Cn().QueryFirstOrDefaultAsync<T>(sql, param, Tx()));

        public async Task<TReturn> QueryFirstOrDefaultAsync<TFirst, TSecond, TReturn>(
             string sql, Func<TFirst, TSecond, TReturn> map, object param = null)
            => (await Cn().QueryAsync<TFirst, TSecond, TReturn>(sql, map, param, Tx())).FirstOrDefault();

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
            string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null,
            IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null) =>
            (await Cn().QueryAsync(sql, map, param as object, transaction, buffered, splitOn, commandTimeout)).AsList();

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
            string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null) =>
            (await Cn().QueryAsync(sql, map, param as object, transaction, buffered, splitOn, commandTimeout)).AsList();

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null) =>
            (await Cn().QueryAsync(sql, map, param as object, transaction, buffered, splitOn, commandTimeout)).AsList();

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null) =>
            (await Cn().QueryAsync(sql, map, param as object, transaction, buffered, splitOn, commandTimeout)).AsList();

        public SqlMapper.GridReader QueryMultiple(
             string sql, object param = null)
            => Cn().QueryMultiple(sql, param, Tx());

        public async Task<SqlMapper.GridReader> QueryMultipleAsync(
             string sql, object param = null)
            => await Cn().QueryMultipleAsync(sql, param, Tx());

        public async Task<int> ExecuteAsync(
             string sql, object param = null)
            => await Cn().ExecuteAsync(sql, param, Tx());

    }
}