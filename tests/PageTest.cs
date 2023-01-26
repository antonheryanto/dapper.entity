using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Dapper.Entity.Tests;

public class PageTest : BaseTest
{
    const string NAME = "PageTest";
    public PageTest() : base(NAME) {}

    [Fact] 
    public async Task PageAsyncTest()
    {
        await InitDb();
        using var db = new Db(_option);
        await db.ExecuteAsync($"INSERT INTO {NAME}(`Url`) VALUES ('#'), ('##')");
        Assert.True((await db.PageAsync<Menu>($"select * from {NAME}")).Items.Count == 2, "paged");
    }

    ~PageTest()
    {
        Drop();
    }
}