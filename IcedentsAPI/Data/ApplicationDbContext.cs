using IncedentsAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;

namespace IncedentsAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
        {
        }

        public DbSet<Account> Accounts { get; set; } = default!;
        public DbSet<Contact> Contacts { get; set; } = default!;
        public DbSet<Incident> Incedents { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Incident>()
                .Property(i => i.Name)
                .ValueGeneratedOnAdd();

            modelBuilder.Entity<Account>()
                .HasKey(a => a.Name);

            modelBuilder.Entity<Account>()
                .HasOne(a => a.Incident)
                .WithMany(i => i.Accounts)
                .HasForeignKey(a => a.IncedentName);

            modelBuilder.Entity<Contact>()
                .HasKey(c => c.Email);

            modelBuilder.Entity<Contact>()
                .HasOne(c => c.Account)
                .WithMany(a => a.Contacts)
                .HasForeignKey(c => c.AccountName);
        }
    }
}
