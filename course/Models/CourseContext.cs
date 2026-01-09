using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace course.Models;

public partial class CourseContext : DbContext
{
    public CourseContext()
    {
    }

    public CourseContext(DbContextOptions<CourseContext> options)
        : base(options)
    {
    }

    public virtual DbSet<About> Abouts { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogComment> BlogComments { get; set; }

    public virtual DbSet<Contact> Contacts { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseCategory> CourseCategories { get; set; }

    public virtual DbSet<CourseReview> CourseReviews { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Instructor> Instructors { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<Menu> Menus { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WebsiteReview> WebsiteReviews { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<About>(entity =>
        {
            entity.HasKey(e => e.AboutId).HasName("PK__About__717FC93CA5AD7548");

            entity.ToTable("About");

            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Assignme__32499E77E1FA4B50");

            entity.Property(e => e.CorrectAnswer).HasMaxLength(255);
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Lesson).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("FK_Assignments_Lessons");
        });

        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blogs__54379E306EB77EB8");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Slug).HasMaxLength(200);
            entity.Property(e => e.Thumbnail).HasMaxLength(255);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Subtitle).HasMaxLength(255);

            entity.HasOne(d => d.User).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Blogs__UserId__4CA06362");
        });

        modelBuilder.Entity<BlogComment>(entity =>
        {
            entity.HasKey(e => e.CommentId).HasName("PK__BlogComm__C3B4DFCAD19DBEFD");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Blog).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.BlogId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogComme__BlogI__5070F446");

            entity.HasOne(d => d.User).WithMany(p => p.BlogComments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogComme__UserI__5165187F");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.HasKey(e => e.ContactId).HasName("PK__Contacts__5C66259BBB26AED5");

            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.SentAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Subject).HasMaxLength(200);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A7BDFFAECF");

            entity.Property(e => e.Alias).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EnrollCount).HasDefaultValue(0);
            entity.Property(e => e.HasCertificate).HasDefaultValue(false);
            entity.Property(e => e.IsFeatured).HasDefaultValue(false);
            entity.Property(e => e.IsNew).HasDefaultValue(false);
            entity.Property(e => e.Level).HasMaxLength(50);
            entity.Property(e => e.Price)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Rating)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(3, 2)");
            entity.Property(e => e.Thumbnail).HasMaxLength(255);
            entity.Property(e => e.Subtitle).HasMaxLength(255);

            entity.Property(e => e.Title).HasMaxLength(200);

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_Courses_CourseCategories");

            entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Courses__Instruc__3F466844");
        });

        modelBuilder.Entity<CourseCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__CourseCa__19093A0BBD3EC19E");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<CourseReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__CourseRe__74BC79CEEC6736B3");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseReviews)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseRev__Cours__66603565");

            entity.HasOne(d => d.User).WithMany(p => p.CourseReviews)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseRev__UserI__656C112C");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771B166E4690");

            entity.Property(e => e.Amount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EnrollDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.HasCertificate).HasDefaultValue(false);
            entity.Property(e => e.PaymentDate).HasColumnType("datetime");
            entity.Property(e => e.Progress).HasDefaultValue(0.0);

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Cours__47DBAE45");

            entity.HasOne(d => d.User).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__UserI__46E78A0C");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.EventId).HasName("PK__Events__7944C810EB88A10A");

            entity.Property(e => e.EventDate).HasColumnType("datetime");
            entity.Property(e => e.Image).HasMaxLength(255);
            entity.Property(e => e.Location).HasMaxLength(200);
            entity.Property(e => e.Title).HasMaxLength(200);

            entity.Property(e => e.TimeFrom).HasMaxLength(20);
            entity.Property(e => e.TimeTo).HasMaxLength(20);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");

            entity.HasOne(e => e.Instructor)
                .WithMany(i => i.Events)     
                .HasForeignKey(e => e.InstructorId)
                .HasConstraintName("FK_Events_Instructors");
        });

        modelBuilder.Entity<Instructor>(entity =>
        {
            entity.HasKey(e => e.InstructorId).HasName("PK__Instruct__9D010A9B253B2C68");

            entity.Property(e => e.Experience).HasMaxLength(255);
            entity.Property(e => e.MainSubject).HasMaxLength(255);
            entity.Property(e => e.Website).HasMaxLength(255);

            entity.Property(e => e.About).HasColumnType("nvarchar(max)");
            entity.HasOne(d => d.User).WithMany(p => p.Instructors)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Instructo__UserI__3C69FB99");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lessons__B084ACD0C76FD64C");

            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.VideoUrl).HasMaxLength(255);

            entity.HasOne(d => d.Course).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Lessons__CourseI__440B1D61");
        });

        modelBuilder.Entity<Menu>(entity =>
        {
            entity.HasKey(e => e.MenuId).HasName("PK__Menu__C99ED2301C35E33C");

            entity.ToTable("Menu");

            entity.Property(e => e.Alias).HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Position).HasMaxLength(50);
            entity.Property(e => e.SortOrder).HasDefaultValue(0);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Url).HasMaxLength(255);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52B997AA5CEE1");

            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.EnrollmentDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.StudentCode)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasComputedColumnSql("('SV'+right('000'+CONVERT([varchar](3),[StudentId]),(3)))", true);

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Students__UserId__03F0984C");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmissionId).HasName("PK__Submissi__449EE1257D9835A9");

            entity.Property(e => e.FilePath).HasMaxLength(500);
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Feedback)
    .HasColumnType("nvarchar(max)");

            entity.Property(e => e.GradedAt)
                .HasColumnType("datetime");

            entity.Property(e => e.Score)
                .HasColumnType("float");

            entity.HasOne(d => d.Assignment).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.AssignmentId)
                .HasConstraintName("FK_Submissions_Assignments");

            entity.HasOne(d => d.Student).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK_Submissions_Students");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CEDE7F0E5");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105343C500413").IsUnique();

            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.Role).HasMaxLength(20);
        });

        modelBuilder.Entity<WebsiteReview>(entity =>
        {
            entity.HasKey(e => e.ReviewId).HasName("PK__WebsiteR__74BC79CECFABF66B");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.User).WithMany(p => p.WebsiteReviews)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__WebsiteRe__UserI__778AC167");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
