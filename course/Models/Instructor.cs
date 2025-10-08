using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Instructor
{
    public int InstructorId { get; set; }

    public int UserId { get; set; }

    public string? Bio { get; set; }

    public string? Experience { get; set; }

    public string? Facebook { get; set; }

    public string? LinkedIn { get; set; }

    public string? Website { get; set; }

    public string? MainSubject { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual User User { get; set; } = null!;
}
