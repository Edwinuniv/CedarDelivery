using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CedarDelivery.Models;

namespace CedarDelivery.Data
{
    // EF Core database context. Inherits the Identity tables (AspNetUsers, AspNetRoles, ...).
    // Phase 1 adds the DbSets and relationships from the requirements PDF.
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
