using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dapper.Entity.Tests
{
    public class TableTest : BaseTest 
    {
        public TableTest() : base() {}

        [Fact] 
        public async Task InsertAsyncTest()
        {
            await InitDb();
            using var db = new Db(_option);
            var m = new Menu { Url = "#" };
            Assert.True((await db.Menu.InsertAsync(m)) == 1, "row inserted");
            Assert.True((await db.Menu.GetAsync(1)).Url == "#", "should has value");
        }

        [Fact] 
        public async Task InsertOrUpdateAsync()
        {
            await InitDb();
            using var db = new Db(_option);
            var a = new Menu { Id = 0, Url = "Insert" };
            a.Id = (int)(await db.Menu.InsertOrUpdateAsync(a.Id, a));
            a.Url = "Update";
            a.Id = (int)(await db.Menu.InsertOrUpdateAsync(a.Id, a));

            Assert.True(a.Id == 1, "row insert or updated");
            Assert.True((await db.Menu.FindAsync(1)).Url == a.Url, "should has value");
        }

        [Fact] 
        public async Task UpdateAsyncTest()
        {
            await InitDb();
            using var db = new Db(_option);
            await db.Menu.InsertAsync(new { Url = "#" });
            Assert.True((await db.Menu.UpdateAsync(1, new { Url = "##" })) == 1, "1 row changes");
            Assert.True((await db.Menu.GetAsync(1)).Url == "##", "value changes");
        } 

        [Fact] 
        public async Task DeleteAsyncTest()
        {
            await InitDb();
            using var db = new Db(_option);
            await db.Menu.InsertAsync(new { Url = "#" });
            Assert.True((await db.Menu.DeleteAsync(1)), "row deleted");
            Assert.Null((await db.Menu.GetAsync(1)));
        }

        ~TableTest()
        {
            Drop();
        }
    }
}