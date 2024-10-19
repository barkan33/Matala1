using Matala1.Models.Entities;
using Matala1.Models.Entitys;
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

        public DbSet<UserButtonClicks> UserButtonClicks { get; set; }
        public DbSet<Button> Buttons { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }
        public DbSet<StudentLesson> StudentLessons { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<WeekDay> WeekDays { get; set; }
        public DbSet<Notice> Notices { get; set; }

    }
}
