using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Assignment
{
    public int AssignmentId { get; set; }

    public int LessonId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? FilePath { get; set; }

    public string? Question { get; set; }

    public string? CorrectAnswer { get; set; }

    public DateTime? Deadline { get; set; }

    public virtual Lesson? Lesson { get; set; }

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
