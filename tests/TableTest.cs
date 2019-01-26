using System.Threading.Tasks;
using Xunit;

namespace Dapper.Entity.Tests
{
    public class TableTest : DatabaseTest 
    {
        public TableTest() : base() {}

        [Fact] 
        public async Task GetAsyncTest()
        {
            using (var db = new Db(_option)) {
                Assert.True((await db.Menu.GetAsync(1)).Url == "#", "should has value");
            }
        } 

    }
}