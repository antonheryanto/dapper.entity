using System.Threading.Tasks;
using Xunit;

namespace Dapper.Entity.Tests
{
    public class TableTest : DatabaseTest 
    {
        public TableTest() : base() {}

        [Fact] 
        public void ToListTest() => Assert.True(db.Menu.ToList().Count > 0, "should has value");

        [Fact] 
        public async Task ToListAsyncTest() => Assert.True((await db.Menu.ToListAsync()).Count > 0, "should has value");

        [Fact] 
        public void GetTest() => Assert.True(db.Menu.Get(4).Url == "#", "should has value");

        [Fact] 
        public async Task GetAsyncTest() => Assert.True((await db.Menu.GetAsync(4)).Url == "#", "should has value");
    }
}