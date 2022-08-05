using Microsoft.EntityFrameworkCore;
using MySuperUniversalBot_BL.Models;

namespace MySuperUniversalBot_BL.Controller
{
    public class DataBaseContextForReminder : DbContext
    {
        public DbSet<Reminder> Reminders => Set<Reminder>();

        public DataBaseContextForReminder() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Reminder.db");
        }

    }
}
