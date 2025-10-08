using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public int InstructorId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public decimal? Price { get; set; }

    public string? Thumbnail { get; set; }

    public string? Level { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? IsFeatured { get; set; }

    public bool? IsNew { get; set; }

    public bool? HasCertificate { get; set; }

    public decimal? Rating { get; set; }

    public int? EnrollCount { get; set; }

    public int? CategoryId { get; set; }

    public string? Alias { get; set; }

    public virtual CourseCategory? Category { get; set; }

    public virtual ICollection<CourseReview> CourseReviews { get; set; } = new List<CourseReview>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual Instructor Instructor { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
