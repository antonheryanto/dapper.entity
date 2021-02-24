using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Dapper.Entity.Tests
{
    public class DatabaseTest : BaseTest
    {
        const string NAME = "DbTest";
        public DatabaseTest() : base(NAME)
        {

        }

        [Fact] 
        public async Task ConnectionTest() 
        {
            await InitDb();
            using var db = new Db(_option);
            var cn = db.Database.GetDbConnection();
            Assert.False(cn is null, $"string {cn} {cn?.State}");
        }

        [Fact] 
        public async Task QueryAsyncTest()
        {
            using var db = new Db(_option);
            Assert.True((await db.QueryAsync("select 1")).Any(), "should has value");
        } 

        [Fact] 
        public async Task ExecuteAsyncTest()
        {
            await InitDb();
            using var db = new Db(_option);
            Assert.True((await db.ExecuteAsync($"Insert Into {NAME} Values(NULL, '#')")) == 1, "row changes");
        } 

        [Fact] 
        public async Task TransactionCommitTest()
        {
            await InitDb();
            using var db = new Db(_option);
            var menu = new Menu { Id = 2, Url = "Transaction Test" };
            using (var tx = db.Database.BeginTransaction()) {
                await db.ExecuteAsync($"Insert Into {NAME} Values(@id, @url)", menu);
                tx.Commit();
            }

            var url = await db.QueryFirstOrDefaultAsync<string>(
                $"Select Url from {NAME} where id=@id", menu);
            Assert.True(url == menu.Url, "row changes");
        } 

        [Fact] 
        public async Task TransactionRollbackTest()
        {
            await InitDb();
            using var db = new Db(_option);
            var menu = new Menu { Id = 2, Url = "Transaction Test" };            
            await db.ExecuteAsync($"Insert Into {NAME} Values(@id, @url)", menu);
            using (var tx = db.Database.BeginTransaction()) {
                try {
                    await db.ExecuteAsync($"Insert Into {NAME} Values(@id, @url)",
                        new { id = menu.Id, url = "Transaction commit" });
                    tx.Commit();
                    throw new Exception();
                } catch (Exception) {
                    tx.Rollback();
                    Assert.True(true, "Rollback");
                }
            }

            var url = await db.QueryFirstOrDefaultAsync<string>(
                $"Select Url from {NAME} where id=@id", menu);
            Assert.True(url == menu.Url, "row changes");
        }

        ~DatabaseTest()
        {
            Drop();
        }
    }
}
