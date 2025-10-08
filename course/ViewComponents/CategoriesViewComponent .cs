using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class CategoriesViewComponent : ViewComponent
{
    private readonly CourseContext _context;

    public CategoriesViewComponent(CourseContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _context.CourseCategories
            .Select(c => new
            {
                c.CategoryId,
                c.Name,
                CourseCount = _context.Courses.Count(course => course.CategoryId == c.CategoryId)
            })
            .ToListAsync();

        return View(categories); // Truyền kiểu List<dynamic>
    }
}
