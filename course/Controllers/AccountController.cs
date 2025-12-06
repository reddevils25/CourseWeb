using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Models;
using System.Security.Cryptography;
using System.Text;

namespace course.Controllers
{
    public class AccountController : Controller
    {
        private readonly CourseContext _context;

        public AccountController(CourseContext context)
        {
            _context = context;
        }

        // GET: Login
        public IActionResult Login()
        {
            // Nếu đã đăng nhập, redirect theo role
            if (HttpContext.Session.GetInt32("UserId").HasValue)
            {
                return RedirectToRoleDashboard();
            }
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            var passwordHash = HashPassword(password);
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash);

            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng";
                return View();
            }

            // Lưu thông tin vào Session
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Email", user.Email);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("Avatar", user.Avatar ?? "/img/instructor/cat.png");

            // Redirect theo role
            return RedirectToRoleDashboard();
        }

        // GET: Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string fullName, string email, string password, string confirmPassword, string role = "Student")
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp";
                return View();
            }

            // Kiểm tra email đã tồn tại
            if (await _context.Users.AnyAsync(u => u.Email == email))
            {
                ViewBag.Error = "Email đã được sử dụng";
                return View();
            }

            var user = new User
            {
                FullName = fullName,
                Email = email,
                PasswordHash = HashPassword(password),
                Role = role, // Default: Student
                CreatedAt = DateTime.Now,
                Avatar = "/images/default-avatar.png"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Nếu đăng ký là Instructor, tạo bản ghi trong bảng Instructors
            if (role == "Instructor")
            {
                var instructor = new Instructor
                {
                    UserId = user.UserId,
                    Bio = "",
                    Experience = "",
                    Facebook = "",
                    LinkedIn = "",
                    Website = "",
                    MainSubject = ""
                };

                _context.Instructors.Add(instructor);
                await _context.SaveChangesAsync();
            }



            ViewBag.Success = "Đăng ký thành công! Vui lòng đăng nhập";
            return View("Login");
        }

        // Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // Access Denied
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Helper: Redirect theo Role
        private IActionResult RedirectToRoleDashboard()
        {
            var role = HttpContext.Session.GetString("Role");
            return role switch
            {
                "Admin" => RedirectToAction("Index", "Home", new { area = "Admin" }),
                "Instructor" => RedirectToAction("Index", "Home", new { area = "Instructor" }),
                "Student" => RedirectToAction("Index", "Home"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // Helper: Hash Password
        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}
