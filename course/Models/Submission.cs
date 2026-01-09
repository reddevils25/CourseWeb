using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Submission
{
    public int SubmissionId { get; set; }

    public int AssignmentId { get; set; }

    public int StudentId { get; set; }

    public string? AnswerText { get; set; }

    public string? FilePath { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }

    public double? Score { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public virtual Assignment Assignment { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
