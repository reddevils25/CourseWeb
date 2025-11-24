using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Models;
using course.Attributes;
using System.Security.Cryptography;
using System.Text;

namespace course.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminAuthorize]
    public class UserManagementController : Controller
    {
        private readonly CourseContext _context;

        public UserManagementController(CourseContext context)
        {
            _context = context;
        }

        // GET: User Management Index
        public async Task<IActionResult> Index(string searchTerm = "", string roleFilter = "", int page = 1)
        {
            int pageSize = 15;
            var query = _context.Users.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(u =>
                    u.FullName.Contains(searchTerm) ||
                    u.Email.Contains(searchTerm));
            }

            // Filter by Role
            if (!string.IsNullOrEmpty(roleFilter))
            {
                query = query.Where(u => u.Role == roleFilter);
            }

            // Pagination
            var totalUsers = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Statistics
            ViewBag.TotalUsers = await _context.Users.CountAsync();
            ViewBag.AdminCount = await _context.Users.CountAsync(u => u.Role == "Admin");
            ViewBag.InstructorCount = await _context.Users.CountAsync(u => u.Role == "Instructor");
            ViewBag.StudentCount = await _context.Users.CountAsync(u => u.Role == "Student");

            ViewBag.SearchTerm = searchTerm;
            ViewBag.RoleFilter = roleFilter;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            return View(users);
        }

        // GET: Create User
        public IActionResult Create()
        {
            ViewBag.Roles = new[] { "Student", "Instructor", "Admin" };
            return View();
        }

        // POST: Create User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user, string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Mật khẩu không được để trống");
                ViewBag.Roles = new[] { "Student", "Instructor", "Admin" };
                return View(user);
            }

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                ViewBag.Roles = new[] { "Student", "Instructor", "Admin" };
                return View(user);
            }

            user.PasswordHash = HashPassword(password);
            user.CreatedAt = DateTime.Now;
            user.Avatar = user.Avatar ?? "/images/default-avatar.png";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Nếu là Instructor, tạo record trong Instructors table
            if (user.Role == "Instructor")
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


            TempData["Success"] = "Tạo người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Edit User
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            ViewBag.Roles = new[] { "Student", "Instructor", "Admin" };
            return View(user);
        }

        // POST: Edit User
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user, string newPassword)
        {
            if (id != user.UserId)
                return NotFound();

            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            // Check email duplicate (except current user)
            if (await _context.Users.AnyAsync(u => u.Email == user.Email && u.UserId != id))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
                ViewBag.Roles = new[] { "Student", "Instructor", "Admin" };
                return View(user);
            }

            existingUser.FullName = user.FullName;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;
            existingUser.Avatar = user.Avatar;

            // Update password if provided
            if (!string.IsNullOrEmpty(newPassword))
            {
                existingUser.PasswordHash = HashPassword(newPassword);
            }

            await _context.SaveChangesAsync();

            // Nếu chuyển sang Instructor mà chưa có record
            var instructor = new Instructor
            {
                UserId = id,
                Bio = "",
                Experience = "",
                Facebook = "",
                LinkedIn = "",
                Website = "",
                MainSubject = ""
            };


            TempData["Success"] = "Cập nhật người dùng thành công!";
            return RedirectToAction(nameof(Index));
        }

        // GET: Delete User
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            return View(user);
        }

        // POST: Delete User Confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound();

            // Không cho xóa chính mình
            var currentUserId = HttpContext.Session.GetInt32("UserId");
            if (currentUserId == id)
            {
                TempData["Error"] = "Không thể xóa tài khoản của chính bạn!";
                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa người dùng thành công!";
            return RedirectToAction(nameof(Index));
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