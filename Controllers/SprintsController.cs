using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDispatcher.Data;
using WebDispatcher.Models;

namespace WebDispatcher.Controllers;

[Authorize]
public class SprintsController : Controller
{
    private readonly ApplicationDbContext _context;

    public SprintsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Create(int projectId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null) return NotFound();
        ViewBag.Project = project;
        return View(new Sprint { ProjectId = projectId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Sprint sprint)
    {
        if (ModelState.IsValid)
        {
            if (sprint.IsActive)
            {
                var active = await _context.Sprints
                    .Where(s => s.ProjectId == sprint.ProjectId && s.IsActive)
                    .ToListAsync();
                foreach (var s in active) s.IsActive = false;
            }

            _context.Sprints.Add(sprint);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Projects", new { id = sprint.ProjectId });
        }

        ViewBag.Project = await _context.Projects.FindAsync(sprint.ProjectId);
        return View(sprint);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(int id)
    {
        var sprint = await _context.Sprints.FindAsync(id);
        if (sprint == null) return NotFound();

        var others = await _context.Sprints
            .Where(s => s.ProjectId == sprint.ProjectId && s.IsActive)
            .ToListAsync();
        foreach (var s in others) s.IsActive = false;

        sprint.IsActive = true;
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", "Projects", new { id = sprint.ProjectId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var sprint = await _context.Sprints.FindAsync(id);
        if (sprint == null) return NotFound();
        var projectId = sprint.ProjectId;
        _context.Sprints.Remove(sprint);
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", "Projects", new { id = projectId });
    }
}
