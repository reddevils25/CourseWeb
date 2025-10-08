using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace course.ViewComponents
{
    public class WebReviewViewComponent : ViewComponent
    {
        private readonly CourseContext _context;

        public WebReviewViewComponent(CourseContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var reviews = await _context.WebsiteReviews
                .Include(r => r.User) // Quan trọng!
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }
    }
}