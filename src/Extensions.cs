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
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dapper.Entity
{
    public static class DbSetExtension
    {
        public static List<T> AsList<T>(this IEnumerable<T> source) =>
            (source == null || source is List<T>) ? (List<T>)source : source.ToList();

        // https://stackoverflow.com/questions/17710769/can-you-get-the-dbcontext-from-a-dbset
        // efcore >= 2.1
        public static Database GetDbContext<T>(this DbSet<T> dbSet) where T : class
            => dbSet.GetService<ICurrentDbContext>().Context as Database;

        public static async Task<List<T>> AllAsync<T>(
            this DbSet<T> dbSet, object where = null) where T : class
            => await dbSet.GetDbContext().AllAsync<T>(where);

        public static async Task<T> GetAsync<T>(
            this DbSet<T> dbSet, object where) where T : class
            => await dbSet.GetDbContext().GetAsync<T>(where);

        public static async Task<T> GetAsync<T>(
            this DbSet<T> dbSet, long id) where T : class
        => await dbSet.GetDbContext().GetAsync<T>(id);

        public static async Task<long> InsertAsync<T>(
            this DbSet<T> dbSet, object data) where T : class
            => await dbSet.GetDbContext().InsertAsync<T>(data);

        public static async Task<long> InsertOrUpdateAsync<T>(
            this DbSet<T> dbSet, object key, object data) where T : class
            => await dbSet.GetDbContext().InsertOrUpdateAsync<T>(key, data);

        public static async Task<long> InsertOrUpdateAsync<T>(
            this DbSet<T> dbSet, long id, object data) where T : class
            => await dbSet.InsertOrUpdateAsync(new { Id = id }, data);

        public static async Task<long> InsertOrUpdateAsync<T>(
            this DbSet<T> dbSet, object data) where T : class
            => await dbSet.GetDbContext().InsertOrUpdateAsync<T>(data);

        public static async Task<int> UpdateAsync<T>(
            this DbSet<T> dbSet, object where, object data) where T : class
            => await dbSet.GetDbContext().UpdateAsync<T>(where, data);

        public static async Task<int> UpdateAsync<T>(
            this DbSet<T> dbSet, long id, object data) where T : class
            => await dbSet.UpdateAsync(new { id }, data);

        public static async Task<bool> DeleteAsync<T>(
            this DbSet<T> dbSet, long id) where T : class
            => await dbSet.GetDbContext().DeleteAsync<T>(id);

        public static async Task<bool> DeleteAsync<T>(
            this DbSet<T> dbSet, object where) where T : class
            => await dbSet.GetDbContext().DeleteAsync<T>(where);
    }
}