using course.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace course.Controllers
{
    public class AssignmentsController : Controller
    {
        private readonly CourseContext _context;

        public AssignmentsController(CourseContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int lessonId)
        {
            var assignments = await _context.Assignments
     .Where(a => a.LessonId == lessonId)
     .Include(a => a.Submissions)
     .ToListAsync();


            ViewBag.Lesson = await _context.Lessons.FindAsync(lessonId);
            return View(assignments);
        }

        [HttpGet]
        public async Task<IActionResult> Submit(int assignmentId)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            return View(assignment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int assignmentId, int studentId, string answerText)
        {
            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                AnswerText = answerText,
                SubmittedAt = DateTime.Now
            };

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new { lessonId = (await _context.Assignments.FindAsync(assignmentId)).LessonId });
        }
    }
}
