using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dapper.Entity.Tests
{
    public class Db : Database
    {
        public Db(DbContextOptions options) : base(options) {}
        public DbSet<Menu> Menu { get; set; }
    }

    public class Menu
    {
        public int Id { get; set; }
        public string Url { get; set; }
        [NotMapped]
        public string Ignore { get; set; }
    }
}