using Microsoft.EntityFrameworkCore;
using System;

namespace Awona.Infastructure
{
    public class Context : DbContext
    {
        public DbSet<Server> Servers { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder option)
            => option.UseMySql("server=localhost;user=root;database=awona;port=3306;Connect Timeout=5;");
    
        public class Server
        {
            public ulong id { get; set; }
            public string prefix { get; set; }

        }
    }

    
}
