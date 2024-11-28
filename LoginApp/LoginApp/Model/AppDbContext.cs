using Microsoft.EntityFrameworkCore;
using LoginApp.Models;
using System.Windows;

namespace LoginApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Reader> Readers { get; set; }
        public DbSet<PenaltyReceipt> PenaltyReceipts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                optionsBuilder.UseSqlServer("Server=LAPTOP-6V6QCQ0J;Database=LoginApp;User Id=sa;Password=123456;TrustServerCertificate=True;");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Database connection error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Password).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(100).IsRequired();
            });
            modelBuilder.Entity<Reader>(entity =>
            {
                entity.HasKey(e => e.MaDocGia);
            });
            modelBuilder.Entity<PenaltyReceipt>(entity =>
            {
                entity.HasKey(e => e.MaPhieuThu);
            });
        }
    }
}