using Matala1.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Matala1.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        // DbSet for User
        public DbSet<User> Users { get; set; }

        // DbSets for other entities (Lecturer, Student, Staff)
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<RoleCode> RoleCodes { get; set; }

        public DbSet<PasswordResetSys> PasswordResetSys { get; set; }
    }
}
