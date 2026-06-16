using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDispatcher.Data;
using WebDispatcher.Models;

namespace WebDispatcher.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return View("Landing");

        return View("Dashboard", await BuildDashboardAsync());
    }

    private async Task<DashboardViewModel> BuildDashboardAsync()
    {
        var userId = _userManager.GetUserId(User)!;

        var recentProjects = await _context.Projects
            .Include(p => p.Issues)
            .Include(p => p.Sprints)
            .OrderByDescending(p => p.CreatedAt)
            .Take(4)
            .ToListAsync();

        var assignedIssues = await _context.Issues
            .Include(i => i.Project)
            .Where(i => i.AssigneeId == userId && i.Status != IssueStatus.Done)
            .OrderByDescending(i => i.UpdatedAt)
            .Take(10)
            .ToListAsync();

        var recentActivity = await _context.IssueActivities
            .Include(a => a.Issue).ThenInclude(i => i.Project)
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .ToListAsync();

        return new DashboardViewModel
        {
            RecentProjects = recentProjects,
            RecentActivity = recentActivity,
            AssignedIssues = assignedIssues,
            AssignedCount = await _context.Issues.CountAsync(i => i.AssigneeId == userId),
            OpenAssignedCount = assignedIssues.Count
        };
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}
