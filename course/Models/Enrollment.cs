using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime? EnrollDate { get; set; }

    public double? Progress { get; set; }

    public bool? HasCertificate { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
