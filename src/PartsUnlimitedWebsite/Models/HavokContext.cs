using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PartsUnlimited.Models
{
    public class HavokContext : DbContext
    {
        public HavokContext()
        {

        }
        public HavokContext(DbContextOptions<HavokContext> options)
            : base(options)
        {
        }

        public DbSet<Havok> Havoks { get; set; }
    }
}

