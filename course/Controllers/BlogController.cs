using course.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace course.Controllers
{
    public class BlogController : Controller
    {
        private readonly CourseContext _context;
        public BlogController(CourseContext context)
        {
            _context = context;
        }

        [Route("/blog/{slug}-{id}.html")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Blogs == null)
            {
                return NotFound();
            }

            // Lấy blog + User tạo blog
            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.BlogId == id);

            if (blog == null)
            {
                return NotFound();
            }

            ViewBag.RelatedBlogs = _context.Blogs
    .Where(b => b.BlogId != id)
    .OrderByDescending(b => b.BlogId)
    .Take(3)
    .ToList();


            ViewBag.Comments = _context.BlogComments
    .Include(c => c.User)
    .Where(c => c.BlogId == id)
    .OrderByDescending(c => c.CreatedAt)
    .ToList();

            ViewBag.TotalComments = ((List<BlogComment>)ViewBag.Comments).Count;


            return View(blog);
        }


        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 9;

            var query = _context.Blogs
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.CreatedAt)
                .AsQueryable();

            int totalPosts = await query.CountAsync();

            var featured = await query.FirstOrDefaultAsync();

            var posts = await query
                .Skip((page - 1) * pageSize + 1) 
                .Take(pageSize)
                .ToListAsync();

            int totalPages = (int)Math.Ceiling((double)(totalPosts - 1) / pageSize);

            return View((featured, posts, page, totalPages));
        }


    }
}
