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
    public partial class Database : DbContext
    {
        static readonly Regex rxColumns =
                new Regex(@"\A\s*SELECT\s+((?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|.)*?)(?<!,\s+)\bFROM\b",
                RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex rxOrderBy = new Regex(
            @"\bORDER\s+BY\s+(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?(?:\s*,\s*(?:\((?>\((?<depth>)|\)(?<-depth>)|.?)*(?(depth)(?!))\)|[\w\(\)\.])+(?:\s+(?:ASC|DESC))?)*",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);
        static readonly Regex rxDistinct = new Regex(@"\ADISTINCT\s",
            RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        async Task<PageParser<T>> PageParseAsync<T>(string sql, int page, dynamic param, int itemsPerPage)
        {
            const int totalPageDisplayed = 9;
            var s = page - totalPageDisplayed;
            if (s <= 0) s = 1;
            //replace SELECT <whatever> => SELECT count(*)
            var m = rxColumns.Match(sql);
            // Save column list and replace with COUNT(*)
            var g = m.Groups[1];
            var sqlSelectRemoved = sql.Substring(g.Index);
            var count = rxDistinct.IsMatch(sqlSelectRemoved) ? m.Groups[1].ToString().Trim() : "*";
            var sqlCount = string.Format("{0} COUNT({1}) {2}", sql.Substring(0, g.Index), count, sql.Substring(g.Index + g.Length));
            // Look for an "ORDER BY <whatever>" clause
            m = rxOrderBy.Match(sqlCount);
            if (m.Success)
            {
                g = m.Groups[0];
                sqlCount = sqlCount.Substring(0, g.Index) + sqlCount.Substring(g.Index + g.Length);
            }
            var total = await QueryFirstOrDefaultAsync<long>(sqlCount, param as object).ConfigureAwait(false);

            var p = new PageParser<T>
            {
                SqlPage = sql + "\n LIMIT @limit OFFSET @offset",
                PageParam = new DynamicParameters(param)
            };
            p.PageParam.Add("@offset", (page - 1) * itemsPerPage);
            p.PageParam.Add("@limit", itemsPerPage);
            var totalPage = total / itemsPerPage;
            if (total % itemsPerPage != 0) totalPage++;
            long pageDisplayed = page + totalPageDisplayed;
            if (pageDisplayed > totalPage) pageDisplayed = totalPage;
            p.Result = new Page<T>
            {
                ItemsPerPage = itemsPerPage,
                CurrentPage = page,
                PageDisplayed = pageDisplayed,
                TotalPage = totalPage,
                Start = s,
                Numbering = (page - 1) * itemsPerPage,
                HasPrevious = page - 1 >= s,
                HasNext = page + 1 <= totalPage,
                TotalItems = total
            };
            return p;
        }

        public async Task<Page<T>> PageAsync<T>(string sql, int page = 1, dynamic param = null, int itemsPerPage = 10)
        {
            var p = await PageParseAsync<T>(sql, page, param as object, itemsPerPage).ConfigureAwait(false);
            var r = await QueryAsync<T>(p.SqlPage, p.PageParam).ConfigureAwait(false);
            p.Result.Items = r.AsList();
            return p.Result;
        }

        public async Task<Page<TReturn>> PageAsync<TFirst, TSecond, TReturn> (
            string sql, Func<TFirst, TSecond, TReturn> map, int page = 1, dynamic param = null, int itemsPerPage = 10, string splitOn = "Id")
        {
            var p = await PageParseAsync<TReturn>(sql, page, param as object, itemsPerPage).ConfigureAwait(false);
            var r = await QueryAsync(p.SqlPage, map, p.PageParam, splitOn: splitOn).ConfigureAwait(false);
            p.Result.Items = r.AsList();
        	return p.Result;
        }

    }

    public class PageParser<T>
    {
        public Page<T> Result { get; set; }
        public string SqlPage { get; set; }
        public DynamicParameters PageParam { get; set; }
    }

    /// <summary>
    /// Paging Class
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Page<T>
    {
        /// <summary>
        /// Gets or sets the items per page.
        /// </summary>
        /// <value>The items per page.</value>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// Gets or sets the current page.
        /// </summary>
        /// <value>The current page.</value>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the page displayed.
        /// </summary>
        /// <value>The page displayed.</value>
        public long PageDisplayed { get; set; }

        /// <summary>
        /// Gets or sets the total page.
        /// </summary>
        /// <value>The total page.</value>
        public long TotalPage { get; set; }

        /// <summary>
        /// Gets or sets the total items.
        /// </summary>
        /// <value>The total items.</value>
        public long TotalItems { get; set; }

        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>The start.</value>
        public int Start { get; set; }

        /// <summary>
        /// Gets or sets the numbering.
        /// </summary>
        /// <value>The numbering.</value>
        public int Numbering { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Dapper.Page`1"/> has previous.
        /// </summary>
        /// <value><c>true</c> if has previous; otherwise, <c>false</c>.</value>
        public bool HasPrevious { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:Dapper.Page`1"/> has next.
        /// </summary>
        /// <value><c>true</c> if has next; otherwise, <c>false</c>.</value>
        public bool HasNext { get; set; }

        /// <summary>
        /// Gets or sets the items.
        /// </summary>
        /// <value>The items.</value>
        public List<T> Items { get; set; }
    }
}