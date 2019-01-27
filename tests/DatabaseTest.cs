using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace Dapper.Entity.Tests
{
    public class DatabaseTest
    {
        protected readonly DbContextOptions _option;

        public DatabaseTest()
        {
            var cs = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json",
                optional: true, reloadOnChange: true)
                .Build()
                .GetConnectionString("db");                
            _option = new DbContextOptionsBuilder<Db>()
                .UseMySql(cs)
                .Options;         
        }

        protected async Task InitDb()
        {
            using (var db = new Db(_option)) {
                await db.ExecuteAsync(@"
CREATE TABLE IF NOT EXISTS `Menu` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `Url` varchar(255) DEFAULT NULL,
    PRIMARY KEY (`Id`)
)ENGINE=MEMORY;
TRUNCATE Menu;
                ");
            }
        }

        [Fact] 
        public async Task ConnectionTest() 
        {
            await InitDb();
            using (var db = new Db(_option)) {
                var cn = db.Database.GetDbConnection();
                Assert.False(cn is null, $"string {cn} {cn?.State}");
            }
        }

        [Fact] 
        public async Task QueryAsyncTest()
        {
            using (var db = new Db(_option)) {
                Assert.True((await db.QueryAsync("select 1")).Any(), "should has value");
            }
        } 

        [Fact] 
        public async Task ExecuteAsyncTest()
        {
            using (var db = new Db(_option)) {
                Assert.True((await db.ExecuteAsync("Insert Into Menu Values(NULL, '#')")) == 1, "row changes");
            }
        } 
    }
}
