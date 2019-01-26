using Dapper;
using System;
using System.Data;
using System.Data.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dapper.Entity
{
    public partial class Database : DbContext
    {
        public Database(DbContextOptions options) : base(options) {}

        public DbConnection cn() => Database.GetDbConnection();

        public DbTransaction tx() => Database.CurrentTransaction?.GetDbTransaction();

        public async Task<List<dynamic>> QueryAsync(string sql, object param = null)
            => (await cn().QueryAsync(sql, param, tx())).AsList();

        public async Task<List<T>> QueryAsync<T>(
             string sql, object param = null)
            => (await cn().QueryAsync<T>(sql, param, tx())).AsList();

        public async Task<T> QueryFirstOrDefaultAsync<T>(
             string sql, object param = null)
            => (await cn().QueryFirstOrDefaultAsync<T>(sql, param, tx()));

        public async Task<TReturn> QueryFirstOrDefaultAsync<TFirst, TSecond, TReturn>(
             string sql, Func<TFirst, TSecond, TReturn> map, object param = null)
            => (await cn().QueryAsync<TFirst, TSecond, TReturn>(sql, map, param, tx())).FirstOrDefault();

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(
            string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null,
            IDbTransaction transaction = null, bool buffered = true,
            string splitOn = "Id", int? commandTimeout = null) =>
            (await cn().QueryAsync(sql, map, param as object, transaction, buffered, splitOn, commandTimeout)).AsList();

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TReturn>(
            string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null) =>
            (await cn().QueryAsync(sql, map, param as object, transaction, buffered, splitOn, commandTimeout)).AsList();

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TReturn>(
            string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null) =>
            (await cn().QueryAsync(sql, map, param as object, transaction, buffered, splitOn, commandTimeout)).AsList();

        public async Task<List<TReturn>> QueryAsync<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(
            string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null,
            IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null) =>
            (await cn().QueryAsync(sql, map, param as object, transaction, buffered, splitOn, commandTimeout)).AsList();

        public SqlMapper.GridReader QueryMultiple(
             string sql, object param = null)
            => cn().QueryMultiple(sql, param, tx());

        public async Task<SqlMapper.GridReader> QueryMultipleAsync(
             string sql, object param = null)
            => await cn().QueryMultipleAsync(sql, param, tx());

        public async Task<int> ExecuteAsync(
             string sql, object param = null)
            => await cn().ExecuteAsync(sql, param, tx());

    }
}