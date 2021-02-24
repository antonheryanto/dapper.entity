using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Dapper.Entity
{
    public partial class Database : DbContext
    {
        public string Esc => DatabaseType switch {
                DatabaseType.MySql => "`",
                _ => "\"",
            };

        public async Task<List<T>> AllAsync<T>(object where = null) where T : class
        {
            var tableName = GetTableName<T>();
            var sql = $"SELECT * FROM {Esc}{tableName}{Esc}";
            if (where == null) return await QueryAsync<T>(sql);

            var paramNames = GetParamNames(where);
            var w = string.Join(" AND ", paramNames.Select(p => $"{Esc}{p}{Esc} = @{p}"));
            return await QueryAsync<T>($"{sql} WHERE {w}", where);
        }

        public async Task<T> GetAsync<T>(object where) where T : class
        {
            var tableName = GetTableName<T>();
            var paramNames = GetParamNames(where);
            var w = string.Join(" AND ", paramNames.Select(p => $"{Esc}{p}{Esc} = @{p}"));
            var sql = $"SELECT * FROM {Esc}{tableName}{Esc} WHERE {w} LIMIT 1";
            return await QueryFirstOrDefaultAsync<T>(sql, where);
        }

        public async Task<T> GetAsync<T>(long id) where T : class
        {
            var tableName = GetTableName<T>();
            var sql = $"SELECT * FROM {Esc}{tableName}{Esc} WHERE id=@id";
            return await QueryFirstOrDefaultAsync<T>(sql, new { id });
        }

        public async Task<long> InsertAsync<T>(object data) where T : class
        {            
            var tableName = GetTableName<T>();
            var paramNames = GetParamNames(data);
            paramNames.Remove("Id");

            string cols = string.Join($"{Esc},{Esc}", paramNames);
            string cols_params = string.Join(",", paramNames.Select(p => $"@{p}"));
            var afterInsert = DatabaseType == DatabaseType.SqlServer ? "Output Inserted.IdentityColumnName" : "";
            var afterValue = DatabaseType switch {
                DatabaseType.PostgreSql => "RETURNING id",
                DatabaseType.MySql => "; SELECT LAST_INSERT_ID()",
                DatabaseType.Sqlite => "; SELECT last_insert_rowid()",
                _ => "",
            };

            var sql = $"INSERT INTO {Esc}{tableName}{Esc} ({Esc}{cols}{Esc}) {afterInsert} VALUES ({cols_params}) {afterValue}";
            return await QueryFirstOrDefaultAsync<long>(sql, data);
        }

        //TODO: sqlite and sqlserver
        public async Task<long> InsertOrUpdateAsync<T>(object key, object data) where T : class
        {
            var tableName = GetTableName<T>();
            string k = GetParamNames(key).Single();  
            var paramNames = GetParamNames(data);
            paramNames.Remove(k);
            string cols = string.Join($"{Esc},{Esc}", paramNames);
            string cols_params = string.Join(",", paramNames.Select(p => $"@{p}"));
            string cols_update = string.Join(",", paramNames.Select(p => $"{Esc}{p}{Esc} = @{p}"));
            var insert = $"INSERT INTO {Esc}{tableName}{Esc} ({Esc}{cols}{Esc},{Esc}{k}{Esc}) VALUES ({cols_params}, @{k})";
            var duplicate = DatabaseType switch {
                DatabaseType.PostgreSql => $"ON CONFLICT (id) SET id = excluded.id, {cols_update} RETURNING id",
                DatabaseType.MySql => $"ON DUPLICATE KEY UPDATE {Esc}{k}{Esc} = LAST_INSERT_ID({Esc}{k}{Esc}), {cols_update}; SELECT LAST_INSERT_ID()",
                _ => "",
            };

            var parameters = new DynamicParameters(data);
            parameters.AddDynamicParams(key);

            return await QueryFirstOrDefaultAsync<long>($"{insert} {duplicate}", parameters);
        }

        public async Task<long> InsertOrUpdateAsync<T>(long id, object data) where T : class
            => await InsertOrUpdateAsync<T>(new { Id = id }, data);

        //TODO: sqlite and sqlserver
        public async Task<long> InsertOrUpdateAsync<T>(object data) where T : class
        {
            var tableName = GetTableName<T>();
            var paramNames = GetParamNames(data);
            string cols = string.Join($"{Esc},{Esc}", paramNames);
            string cols_params = string.Join(",", paramNames.Select(p => $"@{p}"));
            string cols_update = string.Join(",", paramNames.Select(p => $"{Esc}{p}{Esc} = @{p}"));
            var duplicate = DatabaseType switch {
                DatabaseType.PostgreSql => $"ON CONFLICT (id) DO UPDATE SET",
                DatabaseType.MySql => $"ON DUPLICATE KEY UPDATE",
                _ => "",
            };
            var sql = $"INSERT INTO {Esc}{tableName}{Esc} ({Esc}{cols}{Esc}) VALUES ({cols_params}) {duplicate} {cols_update}";

            return await ExecuteAsync(sql, data);
        }

        public async Task<int> UpdateAsync<T>(object where, object data) where T : class
        {
            var tableName = GetTableName<T>();
            var paramNames = GetParamNames(data);
            var keys = GetParamNames(where);
            var cols_update = string.Join(",", paramNames.Select(p => $"{Esc}{p}{Esc}= @{p}"));
            var cols_where = string.Join(" AND ", keys.Select(p => $"{Esc}{p}{Esc} = @{p}"));
            var sql = $"UPDATE {Esc}{tableName}{Esc} SET {cols_update} WHERE {cols_where}";

            var parameters = new DynamicParameters(data);
            parameters.AddDynamicParams(where);
            return await ExecuteAsync(sql, parameters);
        }

        public async Task<int> UpdateAsync<T>(long id, object data) where T : class
            => await UpdateAsync<T>(new { id }, data);

        public async Task<bool> DeleteAsync<T>(long id) where T : class
        {
            var tableName = GetTableName<T>();
            return (await ExecuteAsync(
                $"DELETE FROM {Esc}{tableName}{Esc} WHERE Id = @id", new { id })) > 0;
        }
        public async Task<bool> DeleteAsync<T>(object where) where T : class
        {
            var tableName = GetTableName<T>();
            var paramNames = GetParamNames(where);
            var w = string.Join(" AND ", paramNames.Select(p => $"{Esc}{p}{Esc} = @{p}"));
            return (await ExecuteAsync(
                $"DELETE FROM {Esc}{tableName}{Esc} WHERE {w}", where)) > 0;
        }

        public async Task<int> DeleteAsync<T>( T item) where T : class
        {
            var set = Set<T>();
            set.Remove(item);
            var m = await SaveChangesAsync();
            return m;
        }

        public async Task<int> UpdateAsync<T>( T item) where T : class
        {
            var set = Set<T>();
            set.Remove(item);
            var m = await SaveChangesAsync();
            return m;
        }

        // TODO problem when type is inherit from same type which defined name
        // FIXME efcore >= 1.1
        // https://stackoverflow.com/questions/33051350/get-entity-table-name-ef7
        private static readonly ConcurrentDictionary<Type, string> tableNameMap
            = new ConcurrentDictionary<Type, string>();
        public string GetTableName<T>() where T : class
        {
            var type = typeof(T);
            if (tableNameMap.TryGetValue(type, out string name)) return name;

            //name = Model.FindEntityType(type).Relational().TableName;            
            name = Model.FindEntityType(type).GetTableName();
            if (_lowerCaseTable) name = name?.ToLower();

            tableNameMap[type] = name;
            return name;
        }

        private static readonly ConcurrentDictionary<Type, List<string>> paramNameCache
            = new ConcurrentDictionary<Type, List<string>>();
        public List<string> GetParamNames(object o)
        {
            if (o is DynamicParameters parameters) return parameters.ParameterNames.ToList();
            if (paramNameCache.TryGetValue(o.GetType(), out List<string> paramNames))
                return paramNames;

            paramNames = new List<string>();
            foreach (var prop in o.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.GetGetMethod(false) != null))
            {
                var attribs = prop.GetCustomAttributes(typeof(NotMappedAttribute), true);
                var attr = attribs.FirstOrDefault() as NotMappedAttribute;
                if (attr == null) {
                    paramNames.Add(prop.Name);
                }
            }
            paramNameCache[o.GetType()] = paramNames;
            return paramNames;
        }
    }
}