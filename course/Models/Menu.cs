using System;
using System.Collections.Generic;

namespace course.Models;

public partial class Menu
{
    public int MenuId { get; set; }

    public string Title { get; set; } = null!;

    public string? Alias { get; set; }

    public int? Parent { get; set; }

    public string? Position { get; set; }

    public bool IsActive { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public int? SortOrder { get; set; }
}
