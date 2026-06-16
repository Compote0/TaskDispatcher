using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDispatcher.Data;
using WebDispatcher.Models;

namespace WebDispatcher.Controllers;

[Authorize]
public class IssuesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public IssuesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task PopulateViewBagAsync(int projectId)
    {
        ViewBag.Project = await _context.Projects.FindAsync(projectId);
        var users = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();
        ViewBag.Users = new SelectList(users, "Id", "UserName");
    }

    public async Task<IActionResult> Create(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound();
        await PopulateViewBagAsync(projectId);
        return View(new Issue { ProjectId = projectId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Issue issue)
    {
        if (ModelState.IsValid)
        {
            issue.ReporterId = _userManager.GetUserId(User);
            issue.IssueNumber = await _context.Issues
                .Where(i => i.ProjectId == issue.ProjectId)
                .CountAsync() + 1;
            issue.CreatedAt = DateTime.UtcNow;
            issue.UpdatedAt = DateTime.UtcNow;
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = issue.Id });
        }
        await PopulateViewBagAsync(issue.ProjectId);
        return View(issue);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var issue = await _context.Issues
            .Include(i => i.Project)
            .Include(i => i.Assignee)
            .Include(i => i.Reporter)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (issue == null) return NotFound();
        return View(issue);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var issue = await _context.Issues.Include(i => i.Project).FirstOrDefaultAsync(i => i.Id == id);
        if (issue == null) return NotFound();
        await PopulateViewBagAsync(issue.ProjectId);
        return View(issue);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Issue issue)
    {
        if (id != issue.Id) return NotFound();
        if (ModelState.IsValid)
        {
            issue.UpdatedAt = DateTime.UtcNow;
            _context.Update(issue);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = issue.Id });
        }
        await PopulateViewBagAsync(issue.ProjectId);
        return View(issue);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, IssueStatus status)
    {
        var issue = await _context.Issues.FindAsync(id);
        if (issue == null) return NotFound();
        issue.Status = status;
        issue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", "Projects", new { id = issue.ProjectId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var issue = await _context.Issues.FindAsync(id);
        if (issue == null) return NotFound();
        var projectId = issue.ProjectId;
        _context.Issues.Remove(issue);
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }
}
