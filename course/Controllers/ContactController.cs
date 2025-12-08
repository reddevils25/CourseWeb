using Microsoft.AspNetCore.Mvc;
using course.Models;

namespace course.Controllers
{
    public class ContactController : Controller
    {
        private readonly CourseContext _context;

        public ContactController(CourseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Send(Contact contact)
        {
            if (ModelState.IsValid)
            {
                contact.SentAt = DateTime.Now;
                _context.Contacts.Add(contact);
                _context.SaveChanges();

                ViewBag.Message = "Cảm ơn bạn! Thông tin liên hệ đã được gửi.";

              
                return View("Index", new Contact());
            }

            
            return View("Index", contact);
        }
    }
}
