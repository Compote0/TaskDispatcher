using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebDispatcher.Data;
using WebDispatcher.Models;
using WebDispatcher.Services;

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
            var userId = _userManager.GetUserId(User)!;
            issue.ReporterId = userId;
            issue.IssueNumber = await _context.Issues
                .Where(i => i.ProjectId == issue.ProjectId)
                .CountAsync() + 1;
            issue.CreatedAt = DateTime.UtcNow;
            issue.UpdatedAt = DateTime.UtcNow;
            _context.Issues.Add(issue);
            await _context.SaveChangesAsync();

            await IssueActivityService.LogAsync(_context, issue.Id, userId, ActivityType.Created);
            if (issue.AssigneeId != null)
                await IssueActivityService.LogAsync(_context, issue.Id, userId, ActivityType.Assigned, null, issue.AssigneeId);

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

        ViewBag.Activities = await _context.IssueActivities
            .Include(a => a.User)
            .Where(a => a.IssueId == id)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        ViewBag.Comments = await _context.Comments
            .Include(c => c.Author)
            .Where(c => c.IssueId == id)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentUserId = _userManager.GetUserId(User);

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
            var userId = _userManager.GetUserId(User)!;
            var existing = await _context.Issues.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
            if (existing == null) return NotFound();

            issue.UpdatedAt = DateTime.UtcNow;
            _context.Update(issue);
            await _context.SaveChangesAsync();

            if (existing.Status != issue.Status)
                await IssueActivityService.LogAsync(_context, issue.Id, userId, ActivityType.StatusChanged,
                    existing.Status.ToString(), issue.Status.ToString());
            if (existing.AssigneeId != issue.AssigneeId)
                await IssueActivityService.LogAsync(_context, issue.Id, userId, ActivityType.Assigned,
                    existing.AssigneeId, issue.AssigneeId);
            else
                await IssueActivityService.LogAsync(_context, issue.Id, userId, ActivityType.Updated);

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

        var userId = _userManager.GetUserId(User)!;
        var oldStatus = issue.Status;
        issue.Status = status;
        issue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (oldStatus != status)
            await IssueActivityService.LogAsync(_context, issue.Id, userId, ActivityType.StatusChanged,
                oldStatus.ToString(), status.ToString());

        return RedirectToAction("Details", "Projects", new { id = issue.ProjectId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveStatus([FromBody] MoveStatusDto dto)
    {
        var issue = await _context.Issues.FindAsync(dto.Id);
        if (issue == null) return NotFound();

        var userId = _userManager.GetUserId(User)!;
        var oldStatus = issue.Status;
        issue.Status = dto.Status;
        issue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        if (oldStatus != dto.Status)
            await IssueActivityService.LogAsync(_context, issue.Id, userId,
                ActivityType.StatusChanged, oldStatus.ToString(), dto.Status.ToString());

        return Ok(new { success = true });
    }

    public async Task<IActionResult> Mine()
    {
        var userId = _userManager.GetUserId(User);
        var issues = await _context.Issues
            .Include(i => i.Project)
            .Where(i => i.AssigneeId == userId && i.Status != IssueStatus.Done)
            .OrderBy(i => i.Priority)
            .ThenByDescending(i => i.UpdatedAt)
            .ToListAsync();
        return View(issues);
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

public record MoveStatusDto(int Id, IssueStatus Status);
