using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Entity.Tests;

public class BaseTest
{
    protected readonly DbContextOptions _option;
    protected readonly string _name;
    public BaseTest(string name = "Menu")
    {
        _name = name;
        var cs = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json",
            optional: true, reloadOnChange: true)
            .Build()
            .GetConnectionString("db");
        _option = new DbContextOptionsBuilder<Db>()
            .UseMySql(cs, ServerVersion.AutoDetect(cs))
            .Options;
    }

    protected async Task InitDb()
    {
        using var db = new Db(_option);
        await db.ExecuteAsync($@"
CREATE TABLE IF NOT EXISTS `{_name}` (
    `Id` int(11) NOT NULL AUTO_INCREMENT,
    `Url` varchar(255) DEFAULT NULL,
    PRIMARY KEY (`Id`)
)ENGINE=MEMORY;
TRUNCATE {_name};
                ");
    }       

    protected async void Drop()
    {
        using var db = new Db(_option);
        await db.ExecuteAsync($@"DROP TABLE {_name}");
    }
}
