using Microsoft.AspNetCore.Mvc;

namespace course.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
