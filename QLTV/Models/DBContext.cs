using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;


namespace QLTV.Models
{
    public class DBContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<DOCGIA> Readers { get; set; }
        public DbSet<PHIEUTHUTIENPHAT> PenaltyReceipts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                optionsBuilder.UseSqlServer("Server=LAPTOP-6V6QCQ0J;Database=QLTV;User Id=sa;Password=123456;TrustServerCertificate=True;");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<User>(entity =>
            //{
            //    entity.ToTable("Users");
            //    entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
            //    entity.Property(e => e.Password).HasMaxLength(50).IsRequired();
            //    entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            //});
            modelBuilder.Entity<DOCGIA>(entity =>
            {
                entity.HasKey(e => e.MaDocGia);
            });
            modelBuilder.Entity<PHIEUTHUTIENPHAT>(entity =>
            {
                entity.HasKey(e => e.ID);
            });
        }
    }
}
