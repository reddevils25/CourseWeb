using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace course.ViewComponents
{
    public class MenuTopViewComponent : ViewComponent
    {
        private readonly CourseContext _context;

        public MenuTopViewComponent(CourseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var items = await _context.Menus
                .Where(m => m.IsActive == true && (m.Position == "top" || m.Position == null))
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.Title)
                .ToListAsync();

            return View(items);
        }
    }
}
