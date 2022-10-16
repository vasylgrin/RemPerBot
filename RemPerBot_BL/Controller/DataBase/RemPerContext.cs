using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Models;
using RemBerBot_BL.Models;

namespace RemBerBot_BL.Controller.DataBase
{
    internal class RemPerContext : DbContext
    {
        public DbSet<BirthDate> Reminders => Set<BirthDate>();
        public DbSet<Period> Periods => Set<Period>();
        public DbSet<NotifyTheUser> NotifyTheUsers => Set<NotifyTheUser>();

        public RemPerContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NotifyTheUser>()
            .HasOne(n => n.Period)
            .WithMany(p => p.NotifyTheUser)
            .HasForeignKey(n => n.PeriodId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=RemPer.db");
        }
    }
}
