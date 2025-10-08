using System;
using System.Collections.Generic;

namespace course.Models;

public partial class About
{
    public int AboutId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public string? Image { get; set; }
}
