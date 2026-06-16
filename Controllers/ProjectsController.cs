using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebDispatcher.Data;
using WebDispatcher.Models;

namespace WebDispatcher.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ProjectsController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var projects = await _context.Projects
            .Include(p => p.Issues)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return View(projects);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var project = await _context.Projects
            .Include(p => p.Issues)
                .ThenInclude(i => i.Assignee)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return NotFound();
        return View(project);
    }

    public IActionResult Create() => View(new Project());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        if (ModelState.IsValid)
        {
            project.Key = project.Key.ToUpper();
            project.CreatedAt = DateTime.UtcNow;
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = project.Id });
        }
        return View(project);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var project = await _context.Projects.FindAsync(id);
        if (project == null) return NotFound();
        return View(project);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.Id) return NotFound();
        if (ModelState.IsValid)
        {
            project.Key = project.Key.ToUpper();
            _context.Update(project);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id = project.Id });
        }
        return View(project);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project != null)
        {
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}
