using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dapper.Entity
{
    public abstract partial class Database<TDatabase> : IDisposable where TDatabase : Database<TDatabase>, new()
    {
        /// <summary>
        /// A database table of type <typeparamref name="T"/> and primary key of type <see cref="int"/>.
        /// </summary>
        /// <typeparam name="T">The type of object in this table.</typeparam>
        public class Table<T> : Table<T, int>
        {
            /// <summary>
            /// Creates a table in the specified database with a given name.
            /// </summary>
            /// <param name="database">The database this table belongs in.</param>
            /// <param name="likelyTableName">The name for this table.</param>
			public Table(Database<TDatabase> database, string likelyTableName)
                : base(database, likelyTableName)
            {
            }
        }

        /// <summary>
        /// A database table of type <typeparamref name="T"/> and primary key of type <typeparamref name="TId"/>.
        /// </summary>
        /// <typeparam name="T">The type of object in this table.</typeparam>
        /// <typeparam name="TId">The type of the primary key for this table.</typeparam>
        public partial class Table<T, TId>
        {
            internal Database<TDatabase> database;
            internal string tableName;
            internal string likelyTableName;

            /// <summary>
            /// Creates a table in the specified database with a given name.
            /// </summary>
            /// <param name="database">The database this table belongs in.</param>
            /// <param name="likelyTableName">The name for this table.</param>
            public Table(Database<TDatabase> database, string likelyTableName)
            {
                this.database = database;
                this.likelyTableName = likelyTableName;
            }

            /// <summary>
            /// The name for this table.
            /// </summary>
            public string TableName {
                get {
                    tableName = tableName ?? database.DetermineTableName<T>(likelyTableName);
                    return tableName;
                }
            }

            /// <summary>
            /// Insert a row into the db
            /// </summary>
            /// <param name="data">Either DynamicParameters or an anonymous type or concrete type</param>
            /// <returns></returns>
            public virtual int? Insert(dynamic data)
            {
                var o = (object)data;
                List<string> paramNames = GetParamNames(o);
                paramNames.Remove("Id");

                string cols = string.Join(",", paramNames);
                string colsParams = string.Join(",", paramNames.Select(p => "@" + p));
                var sql = $"set nocount on insert {TableName} ({cols}) values ({colsParams}) select cast(scope_identity() as int)";

                return database.Query<int?>(sql, o).Single();
            }

            /// <summary>
            /// Update a record in the database.
            /// </summary>
            /// <param name="id">The primary key of the row to update.</param>
            /// <param name="data">The new object data.</param>
            /// <returns>The number of rows affected.</returns>
            public int Update(TId id, dynamic data)
            {
                List<string> paramNames = GetParamNames((object)data);

                var builder = new StringBuilder();
                builder.Append("update ").Append(TableName).Append(" set ");
                builder.AppendLine(string.Join(",", paramNames.Where(n => n != "Id").Select(p => p + "= @" + p)));
                builder.Append("where Id = @Id");

                DynamicParameters parameters = new DynamicParameters(data);
                parameters.Add("Id", id);

                return database.Execute(builder.ToString(), parameters);
            }

            /// <summary>
            /// Delete a record for the DB
            /// </summary>
            /// <param name="id"></param>
            /// <returns></returns>
            public bool Delete(TId id)
            {
                return database.Execute($"delete from {TableName} where Id = @id", new { id }) > 0;
            }

            /// <summary>
            /// Gets a record with a particular Id from the DB 
            /// </summary>
            /// <param name="id">The primary key of the table to fetch.</param>
            /// <returns>The record with the specified Id.</returns>
            public T Get(TId id)
            {
                return database.QueryFirstOrDefault<T>($"select * from {TableName} where Id = @id", new { id });
            }

            /// <summary>
            /// Gets the first row from this table (order determined by the database provider).
            /// </summary>
            /// <returns>Data from the first table row.</returns>
            public virtual T First()
            {
                return database.QueryFirstOrDefault<T>($"select top 1 * from {TableName}");
            }

            /// <summary>
            /// Gets the all rows from this table.
            /// </summary>
            /// <returns>Data from all table rows.</returns>
            public List<T> ToList()
            {
                return database.Query<T>($"select * from {TableName}").ToList();
            }

            private static readonly ConcurrentDictionary<Type, List<string>> paramNameCache = new ConcurrentDictionary<Type, List<string>>();

            internal static List<string> GetParamNames(object o)
            {
                if (o is DynamicParameters parameters) {
                    return parameters.ParameterNames.ToList();
                }

                if (!paramNameCache.TryGetValue(o.GetType(), out List<string> paramNames)) {
                    paramNames = new List<string>();
                    foreach (var prop in o.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.GetGetMethod(false) != null)) {
                        var attribs = prop.GetCustomAttributes(typeof(IgnorePropertyAttribute), true);
                        var attr = attribs.FirstOrDefault() as IgnorePropertyAttribute;
                        if (attr == null || (!attr.Value)) {
                            paramNames.Add(prop.Name);
                        }
                    }
                    paramNameCache[o.GetType()] = paramNames;
                }
                return paramNames;
            }
        }
    }
}