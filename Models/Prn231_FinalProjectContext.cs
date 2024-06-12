using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FinalProject.Models
{
    public partial class Prn231_FinalProjectContext : DbContext
    {
        public Prn231_FinalProjectContext()
        {
        }

        public Prn231_FinalProjectContext(DbContextOptions<Prn231_FinalProjectContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; } = null!;
        public virtual DbSet<Course> Courses { get; set; } = null!;
        public virtual DbSet<CourseCategory> CourseCategories { get; set; } = null!;
        public virtual DbSet<CourseEnrolled> CourseEnrolleds { get; set; } = null!;
        public virtual DbSet<Lesson> Lessons { get; set; } = null!;
        public virtual DbSet<Mooc> Moocs { get; set; } = null!;
        public virtual DbSet<Token> Tokens { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserCourse> UserCourses { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();
                optionsBuilder.UseSqlServer(config.GetConnectionString("Value"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Href).HasMaxLength(255);

                entity.Property(e => e.Name).HasMaxLength(255);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(500);

                entity.Property(e => e.Href).HasMaxLength(500);

                entity.Property(e => e.Image)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Name).HasMaxLength(500);

                entity.Property(e => e.UpdateAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<CourseCategory>(entity =>
            {
                entity.ToTable("Course_Category");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CategoryId).HasColumnName("CategoryID");

                entity.Property(e => e.CourseId).HasColumnName("CourseID");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.CourseCategories)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK__Course_Ca__Categ__44FF419A");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.CourseCategories)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK__Course_Ca__Cours__440B1D61");
            });

            modelBuilder.Entity<CourseEnrolled>(entity =>
            {
                entity.ToTable("Course_Enrolled");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CourseId).HasColumnName("CourseID");

                entity.Property(e => e.Href).HasMaxLength(500);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.CourseEnrolleds)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK__Course_En__Cours__4E88ABD4");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.CourseEnrolleds)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Course_En__UserI__4D94879B");
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.ToTable("Lesson");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Href).HasMaxLength(255);

                entity.Property(e => e.MoocId).HasColumnName("MoocID");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.Property(e => e.UpdateAt).HasColumnType("datetime");

                entity.Property(e => e.VideoTranscript).HasColumnType("ntext");

                entity.Property(e => e.VideoUrl).HasMaxLength(500);

                entity.HasOne(d => d.Mooc)
                    .WithMany(p => p.Lessons)
                    .HasForeignKey(d => d.MoocId)
                    .HasConstraintName("FK__Lesson__MoocID__4AB81AF0");
            });

            modelBuilder.Entity<Mooc>(entity =>
            {
                entity.ToTable("Mooc");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CourseId).HasColumnName("CourseID");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Moocs)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK__Mooc__CourseID__47DBAE45");
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.ToTable("Token");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.ExpiredTime).HasColumnType("datetime");

                entity.Property(e => e.TokenValue)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Type)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tokens)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__Token__UserID__398D8EEE");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Email)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.FullName).HasMaxLength(255);

                entity.Property(e => e.Password)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Role)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.TypeAuthentication)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.UpdateAt).HasColumnType("datetime");
            });

            modelBuilder.Entity<UserCourse>(entity =>
            {
                entity.ToTable("User_Course");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.CourseId).HasColumnName("CourseID");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.UserCourses)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK__User_Cour__Cours__3F466844");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.UserCourses)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("FK__User_Cour__UserI__3E52440B");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
