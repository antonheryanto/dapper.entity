using System;

namespace Dapper.Entity.Tests
{
    public class Db : Database<Db>
    {
        public Table<Menu> Menu { get; set; }
    }

    public class Menu
    {
        public int Id { get; set; }
        public string Url { get; set; }
    }
}