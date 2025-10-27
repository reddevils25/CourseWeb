using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Student
{
    public int StudentId { get; set; }

    public int UserId { get; set; }

    public string? StudentCode { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public DateTime? EnrollmentDate { get; set; }

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
