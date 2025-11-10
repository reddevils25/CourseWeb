using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? VideoUrl { get; set; }

    public int? Duration { get; set; }

    public string? Content { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual Course Course { get; set; } = null!;
}
