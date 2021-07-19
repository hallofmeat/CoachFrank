using CoachFrank.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CoachFrank.Data
{
    public class BotContext : DbContext
    {
        public BotContext(DbContextOptions<BotContext> options)
            : base(options)
        {
        }

        public DbSet<Warning> Warnings { get; set; }
    }
}
