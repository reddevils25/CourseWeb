using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Contact
{
    public int ContactId { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Subject { get; set; }

    public string? Message { get; set; }

    public DateTime? SentAt { get; set; }
}
