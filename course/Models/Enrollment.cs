using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int UserId { get; set; }

    public int CourseId { get; set; }

    public DateTime EnrollDate { get; set; }

    public double? Progress { get; set; }

    public bool? HasCertificate { get; set; }

    public decimal? Amount { get; set; }

    public bool IsPaid { get; set; }

    public DateTime? PaymentDate { get; set; }

    [ValidateNever]
    public virtual Course Course { get; set; } = null!;

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
