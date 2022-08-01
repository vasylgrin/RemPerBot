using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Models;

namespace MySuperUniversalBot_BL.Controller
{
    public class DataBaseContextForPeriod : DbContext
    {
        public DbSet<Period> Periods => Set<Period>();
        public DbSet<NotifyTheUser> NotifyTheUsers => Set<NotifyTheUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<NotifyTheUser>()
            .HasOne(n => n.Period)
            .WithMany(p => p.NotifyTheUser)
            .HasForeignKey(n => n.PeriodId);
        }

        public DataBaseContextForPeriod() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Period.db");

        }
    }
}
