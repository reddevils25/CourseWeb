using course.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace course.Areas.Admin.Controllers
{
    public class HomeController : Controller
    {
        [Area("Admin")]
        [AdminAuthorize]
        [InstructorAuthorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
