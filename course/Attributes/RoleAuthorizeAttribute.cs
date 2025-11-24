using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace course.Attributes
{
    public class RoleAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public RoleAuthorizeAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Kiểm tra đã đăng nhập chưa
            var userId = context.HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { area = "" });
                return;
            }

            // Kiểm tra role
            var userRole = context.HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", new { area = "" });
                return;
            }
        }
    }

    // Shortcut attributes
    public class AdminAuthorizeAttribute : RoleAuthorizeAttribute
    {
        public AdminAuthorizeAttribute() : base("Admin") { }
    }

    public class InstructorAuthorizeAttribute : RoleAuthorizeAttribute
    {
        public InstructorAuthorizeAttribute() : base("Instructor", "Admin") { }
    }

    public class StudentAuthorizeAttribute : RoleAuthorizeAttribute
    {
        public StudentAuthorizeAttribute() : base("Student", "Instructor", "Admin") { }
    }
}
