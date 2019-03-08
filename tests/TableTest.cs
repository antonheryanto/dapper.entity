using System.Threading.Tasks;
using Xunit;

namespace Dapper.Entity.Tests
{
    public class TableTest : DatabaseTest 
    {
        public TableTest() : base() {}

        [Fact] 
        public async Task InsertAsyncTest()
        {
            await InitDb();
            using (var db = new Db(_option)) {
                var m = new Menu { Url = "#" };
                Assert.True((await db.Menu.InsertAsync(m)) == 1, "row inserted");
                Assert.True((await db.Menu.GetAsync(1)).Url == "#", "should has value");
            }
        } 

        [Fact] 
        public async Task UpdateAsyncTest()
        {
            using (var db = new Db(_option)) {
                Assert.True((await db.Menu.UpdateAsync(1, new { Url = "##"})) == 1, "1 row changes");
                Assert.True((await db.Menu.GetAsync(1)).Url == "##", "value changes");
            }
        } 

        [Fact] 
        public async Task DeleteAsyncTest()
        {
            using (var db = new Db(_option)) {
                Assert.True((await db.Menu.DeleteAsync(1)), "row deleted");
                Assert.Null((await db.Menu.GetAsync(1)));
            }
        } 

    }
}