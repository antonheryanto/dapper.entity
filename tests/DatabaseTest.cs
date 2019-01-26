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

        async Task InitDb()
        {
            using (var db = new Db(_option)) {
                await db.ExecuteAsync(@"
DROP TABLE IF EXISTS `Menu`;
create table Menu (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `Url` varchar(255) DEFAULT NULL,
    PRIMARY KEY (`Id`)
);                
insert into Menu values(NULL, '#');
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
        public async Task QueryTest()
        {
            using (var db = new Db(_option)) {
                Assert.True((await db.QueryAsync("select 1")).Any(), "should has value");
            }
        } 
    }
}
