using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using course.Models;

namespace course.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class StatisticsController : Controller
    {
        private readonly CourseContext _context;

        public StatisticsController(CourseContext context)
        {
            _context = context;
        }

        public IActionResult Index(string timeRange = "all", DateTime? startDate = null, DateTime? endDate = null, int? categoryId = null, string level = null)
        {
            // Xác định khoảng thời gian
            DateTime filterStartDate = startDate ?? DateTime.MinValue;
            DateTime filterEndDate = endDate ?? DateTime.MaxValue;

            switch (timeRange)
            {
                case "today":
                    filterStartDate = DateTime.Today;
                    filterEndDate = DateTime.Today.AddDays(1);
                    break;
                case "week":
                    filterStartDate = DateTime.Today.AddDays(-7);
                    filterEndDate = DateTime.Today.AddDays(1);
                    break;
                case "month":
                    filterStartDate = DateTime.Today.AddMonths(-1);
                    filterEndDate = DateTime.Today.AddDays(1);
                    break;
                case "year":
                    filterStartDate = DateTime.Today.AddYears(-1);
                    filterEndDate = DateTime.Today.AddDays(1);
                    break;
                case "custom":
                    filterStartDate = startDate ?? DateTime.MinValue;
                    filterEndDate = endDate ?? DateTime.MaxValue;
                    break;
            }

            ViewBag.TimeRange = timeRange;
            ViewBag.StartDate = filterStartDate != DateTime.MinValue ? filterStartDate.ToString("yyyy-MM-dd") : "";
            ViewBag.EndDate = filterEndDate != DateTime.MaxValue ? filterEndDate.ToString("yyyy-MM-dd") : "";
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedLevel = level;

            // Danh sách cho filter
            ViewBag.Categories = _context.CourseCategories.OrderBy(c => c.Name).ToList();
            ViewBag.Levels = new List<string> { "Beginner", "Intermediate", "Advanced" };

            // Query cơ bản với filter
            var coursesQuery = _context.Courses.AsQueryable();
            var enrollmentsQuery = _context.Enrollments.AsQueryable();

            if (categoryId.HasValue)
                coursesQuery = coursesQuery.Where(c => c.CategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(level))
                coursesQuery = coursesQuery.Where(c => c.Level == level);

            if (filterStartDate != DateTime.MinValue || filterEndDate != DateTime.MaxValue)
            {
                enrollmentsQuery = enrollmentsQuery.Where(e => 
                    e.EnrollDate >= filterStartDate && e.EnrollDate < filterEndDate);
            }

            // ========== KPI CARDS (Filtered) ==========
            ViewBag.TotalCourses = coursesQuery.Count();
            ViewBag.TotalCategories = _context.CourseCategories.Count();
            ViewBag.TotalFeatured = coursesQuery.Count(c => c.IsFeatured == true);
            ViewBag.TotalEnrollments = enrollmentsQuery.Count();
            ViewBag.TotalRevenue = enrollmentsQuery.Where(e => e.IsPaid).Sum(e => e.Amount);
            ViewBag.TotalStudents = enrollmentsQuery.Select(e => e.UserId).Distinct().Count();
            
            var coursesWithRating = coursesQuery.Where(c => c.Rating > 0);
            ViewBag.AverageRating = coursesWithRating.Any() ? coursesWithRating.Average(c => c.Rating) : 0;
            ViewBag.CoursesWithCertificate = coursesQuery.Count(c => c.HasCertificate);
            ViewBag.AveragePrice = coursesQuery.Any() ? coursesQuery.Average(c => c.Price) : 0;

            // ========== COMPARISON WITH PREVIOUS PERIOD (SQL-SAFE) ==========
            TimeSpan period = filterEndDate - filterStartDate;
            DateTime sqlMin = new DateTime(1753, 1, 1);
            DateTime sqlMax = new DateTime(9999, 12, 31);

            // prevEndDate là filterStartDate, nhưng phải nằm trong SQL range
            DateTime prevEndDate = filterStartDate < sqlMin ? sqlMin : filterStartDate > sqlMax ? sqlMax : filterStartDate;

            // prevStartDate an toàn: filterStartDate - period, nhưng >= sqlMin
            DateTime candidatePrevStart;
            try
            {
                candidatePrevStart = filterStartDate - period;
            }
            catch
            {
                candidatePrevStart = sqlMin;
            }

            DateTime prevStartDate = candidatePrevStart < sqlMin ? sqlMin : candidatePrevStart;

            // Lọc enrollments
            var prevEnrollments = _context.Enrollments
                .Where(e => e.EnrollDate >= prevStartDate && e.EnrollDate < prevEndDate);



            // Tính số lượng đăng ký hiện tại và trước đó
            int currentEnrollCount = enrollmentsQuery.Count();
            int prevEnrollCount = prevEnrollments.Count();
            ViewBag.EnrollmentGrowth = prevEnrollCount > 0
                ? Math.Round((double)(currentEnrollCount - prevEnrollCount) / prevEnrollCount * 100, 2)
                : 0;

            // Tính doanh thu hiện tại và trước đó
            decimal currentRevenue = enrollmentsQuery
                .Where(e => e.IsPaid)
                .Sum(e => e.Amount ?? 0);

            decimal prevRevenue = prevEnrollments
                .Where(e => e.IsPaid)
                .Sum(e => e.Amount ?? 0);

            ViewBag.RevenueGrowth = prevRevenue > 0
                ? Math.Round((double)(currentRevenue - prevRevenue) / (double)prevRevenue * 100, 2)
                : 0;


            // ========== DAILY STATISTICS (Last 30 days) ==========
            var last30Days = Enumerable.Range(0, 30)
                .Select(i => DateTime.Today.AddDays(-29 + i))
                .ToList();

            ViewBag.DailyLabels = last30Days.Select(d => d.ToString("dd/MM")).ToList();
            ViewBag.DailyEnrollments = last30Days.Select(day =>
                _context.Enrollments.Count(e => e.EnrollDate.Date == day.Date)
            ).ToList();
            ViewBag.DailyRevenue = last30Days.Select(day =>
                _context.Enrollments
                    .Where(e => e.EnrollDate.Date == day.Date && e.IsPaid)
                    .Sum(e => e.Amount)
            ).ToList();

            // ========== HOURLY STATISTICS (Today) ==========
            var hours = Enumerable.Range(0, 24).ToList();
            ViewBag.HourlyLabels = hours.Select(h => $"{h}:00").ToList();
            ViewBag.HourlyEnrollments = hours.Select(hour =>
                _context.Enrollments.Count(e => 
                    e.EnrollDate.Date == DateTime.Today && 
                    e.EnrollDate.Hour == hour)
            ).ToList();

            // ========== MONTHLY STATISTICS (Last 12 months) ==========
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.Now.AddMonths(-11 + i))
                .ToList();

            ViewBag.MonthlyLabels = last12Months.Select(d => d.ToString("MM/yyyy")).ToList();
            ViewBag.MonthlyEnrollments = last12Months.Select(month =>
                _context.Enrollments.Count(e =>
                    e.EnrollDate.Month == month.Month &&
                    e.EnrollDate.Year == month.Year)
            ).ToList();
            ViewBag.MonthlyRevenue = last12Months.Select(month =>
                _context.Enrollments
                    .Where(e => e.IsPaid &&
                                e.PaymentDate.HasValue &&
                                e.PaymentDate.Value.Month == month.Month &&
                                e.PaymentDate.Value.Year == month.Year)
                    .Sum(e => e.Amount)
            ).ToList();

            // ========== DAY OF WEEK STATISTICS ==========

            // Lấy tất cả enrollments trước, chỉ cần EnrollDate
            var allEnrollments = _context.Enrollments
                .Select(e => e.EnrollDate)
                .AsEnumerable() // <-- chuyển sang xử lý client
                .ToList();

            var daysOfWeek = Enumerable.Range(0, 7).ToArray(); // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
            ViewBag.DayOfWeekLabels = new[] { "CN", "T2", "T3", "T4", "T5", "T6", "T7" };

            ViewBag.DayOfWeekEnrollments = daysOfWeek.Select(dow =>
                allEnrollments.Count(d => (int)d.DayOfWeek == dow)
            ).ToList();



            // ========== CATEGORY STATISTICS ==========
            var categoryStats = _context.CourseCategories
                .Select(cat => new
                {
                    Name = cat.Name,
                    Count = coursesQuery.Count(c => c.CategoryId == cat.CategoryId),
                    Enrollments = enrollmentsQuery.Count(e => e.Course.CategoryId == cat.CategoryId),
                    Revenue = enrollmentsQuery
                        .Where(e => e.Course.CategoryId == cat.CategoryId && e.IsPaid)
                        .Sum(e => e.Amount),
                    AvgRating = coursesQuery
                        .Where(c => c.CategoryId == cat.CategoryId && c.Rating > 0)
                        .Average(c => (double?)c.Rating) ?? 0
                })
                .OrderByDescending(x => x.Enrollments)
                .ToList();

            ViewBag.CategoryLabels = categoryStats.Select(x => x.Name).ToList();
            ViewBag.CategoryCounts = categoryStats.Select(x => x.Count).ToList();
            ViewBag.CategoryEnrollments = categoryStats.Select(x => x.Enrollments).ToList();
            ViewBag.CategoryRevenue = categoryStats.Select(x => x.Revenue).ToList();

            // ========== LEVEL STATISTICS ==========
            var levelStats = coursesQuery
                .GroupBy(c => c.Level)
                .Select(g => new
                {
                    Level = g.Key,
                    Count = g.Count(),
                    AvgPrice = g.Average(c => c.Price),
                    AvgEnroll = g.Average(c => c.EnrollCount),
                    AvgRating = g.Where(c => c.Rating > 0).Average(c => (double?)c.Rating) ?? 0
                })
                .ToList();

            ViewBag.LevelLabels = levelStats.Select(x => x.Level).ToList();
            ViewBag.LevelCounts = levelStats.Select(x => x.Count).ToList();
            ViewBag.LevelAvgPrice = levelStats.Select(x => x.AvgPrice).ToList();

            // ========== INSTRUCTOR STATISTICS ==========
            ViewBag.TopInstructors = _context.Courses
                .Where(c => coursesQuery.Select(cq => cq.CourseId).Contains(c.CourseId))
                .GroupBy(c => new { c.InstructorId, c.Instructor.User.FullName })
                .Select(g => new
                {
                    InstructorName = g.Key.FullName,
                    TotalCourses = g.Count(),
                    TotalEnrollments = g.Sum(c => c.EnrollCount),
                    AvgRating = g.Where(c => c.Rating > 0).Average(c => (double?)c.Rating) ?? 0,
                    TotalRevenue = enrollmentsQuery
                        .Where(e => e.Course.InstructorId == g.Key.InstructorId && e.IsPaid)
                        .Sum(e => e.Amount)
                })
                .OrderByDescending(x => x.TotalEnrollments)
                .Take(10)
                .ToList();

            // ========== PRICE RANGE STATISTICS ==========
            var priceRanges = new[]
            {
                new { Label = "Miễn phí", Min = 0m, Max = 0m },
                new { Label = "< 500K", Min = 1m, Max = 500000m },
                new { Label = "500K - 1M", Min = 500000m, Max = 1000000m },
                new { Label = "1M - 2M", Min = 1000000m, Max = 2000000m },
                new { Label = "> 2M", Min = 2000000m, Max = decimal.MaxValue }
            };

            ViewBag.PriceRangeLabels = priceRanges.Select(r => r.Label).ToList();
            var allCourses = coursesQuery
            .Select(c => c.Price)
            .AsEnumerable()  
            .ToList();

            ViewBag.PriceRangeCounts = priceRanges.Select(r =>
                allCourses.Count(p =>
                    (r.Min == 0 && r.Max == 0 && p == 0) ||
                    (r.Min > 0 && p >= r.Min && p <= r.Max))
            ).ToList();


            // ========== COMPLETION RATE ==========
            ViewBag.CompletionStats = new
            {
                TotalEnrollments = enrollmentsQuery.Count(),
                Completed = enrollmentsQuery.Count(e => e.Progress >= 100),
                InProgress = enrollmentsQuery.Count(e => e.Progress > 0 && e.Progress < 100),
                NotStarted = enrollmentsQuery.Count(e => e.Progress == 0),
                AvgProgress = enrollmentsQuery.Any() ? enrollmentsQuery.Average(e => e.Progress) : 0
            };

            // ========== PAYMENT STATISTICS ==========
            ViewBag.PaymentStats = new
            {
                Paid = enrollmentsQuery.Count(e => e.IsPaid),
                Unpaid = enrollmentsQuery.Count(e => !e.IsPaid),
                WithCertificate = enrollmentsQuery.Count(e => e.HasCertificate == true),
                ConversionRate = enrollmentsQuery.Any() 
                    ? Math.Round((double)enrollmentsQuery.Count(e => e.IsPaid) / enrollmentsQuery.Count() * 100, 2)
                    : 0
            };

            // ========== TOP COURSES ==========
            ViewBag.TopCoursesByEnroll = coursesQuery
                .OrderByDescending(c => c.EnrollCount)
                .Take(10)
                .Include(c => c.Instructor).ThenInclude(i => i.User)
                .Include(c => c.Category)
                .Select(c => new
                {
                    c.Title,
                    InstructorName = c.Instructor.User.FullName,
                    c.EnrollCount,
                    c.Level,
                    c.Rating,
                    CategoryName = c.Category.Name,
                    c.Price,
                    Revenue = enrollmentsQuery
                        .Where(e => e.CourseId == c.CourseId && e.IsPaid)
                        .Sum(e => e.Amount)
                })
                .ToList();

            ViewBag.TopCoursesByRevenue = coursesQuery
                .Select(c => new
                {
                    c.CourseId,
                    c.Title,
                    InstructorName = c.Instructor.User.FullName,
                    c.EnrollCount,
                    c.Price,
                    c.Rating,
                    Revenue = enrollmentsQuery
                        .Where(e => e.CourseId == c.CourseId && e.IsPaid)
                        .Sum(e => e.Amount)
                })
                .OrderByDescending(x => x.Revenue)
                .Take(10)
                .ToList();

            // ========== RECENT ENROLLMENTS ==========
            ViewBag.RecentEnrollments = enrollmentsQuery
                .OrderByDescending(e => e.EnrollDate)
                .Take(15)
                .Include(e => e.Course)
                .Include(e => e.User)
                .Select(e => new
                {
                    StudentName = e.User.FullName,
                    CourseName = e.Course.Title,
                    e.EnrollDate,
                    e.Amount,
                    e.IsPaid,
                    e.Progress
                })
                .ToList();

            // ========== STUDENT RETENTION ==========
            var activeStudents = enrollmentsQuery
                .Where(e => e.EnrollDate >= DateTime.Today.AddDays(-30))
                .Select(e => e.UserId)
                .Distinct()
                .Count();
            
            var returningStudents = enrollmentsQuery
                .Where(e => e.EnrollDate >= DateTime.Today.AddDays(-30))
                .GroupBy(e => e.UserId)
                .Count(g => g.Count() > 1);

            ViewBag.StudentRetention = new
            {
                ActiveStudents = activeStudents,
                ReturningStudents = returningStudents,
                RetentionRate = activeStudents > 0 
                    ? Math.Round((double)returningStudents / activeStudents * 100, 2)
                    : 0
            };

            return View();
        }
    }
}