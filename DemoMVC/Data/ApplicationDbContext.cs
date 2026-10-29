
using DemoMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Bảng sinh viên
        public DbSet<Student> Students { get; set; }

        // Bảng khoa
        public DbSet<Department> Departments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Quan hệ khóa ngoại giữa Sinh viên và Khoa
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Department)
                .WithMany(d => d.Students)
                .HasForeignKey(s => s.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Mã sinh viên không được trùng nhau
            modelBuilder.Entity<Student>()
                .Property(s => s.StudentCode)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<Student>()
                .HasIndex(s => s.StudentCode)
                .IsUnique();
        }
    }
}


