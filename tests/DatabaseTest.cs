using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit;
using System.Data.Common;

namespace Dapper.Entity.Tests
{
    public class DatabaseTest
    {
        protected readonly Db db;
        private readonly string cs;
        private readonly DbConnection cn;

        public IConfigurationRoot Configuration { get; set; }

        public DatabaseTest()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json",
                optional: true, reloadOnChange: true);
            Configuration = builder.Build();

            cs = Configuration.GetConnectionString("db");                        
            cn = new System.Data.SqlClient.SqlConnection(cs);
            db = Db.Init(cn, 30);
        }

        [Fact] 
        public void ConnectionStringTest() => Assert.False(cs is null, "not found");

        [Fact] 
        public void ConnectionTest() => Assert.False(cn is null, $"string {cn} {cn?.State}");

        [Fact] 
        public void DbTest() => Assert.False(db is null, "cannot init");
            
        [Fact] 
        public void QueryTest() => Assert.True(db.Query("select 1").Any(), "should has value");
    }
}
