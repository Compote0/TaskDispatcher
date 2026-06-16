using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebDispatcher.Data;
using WebDispatcher.Models;

namespace WebDispatcher.Controllers;

[Authorize]
public class CommentsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public CommentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int issueId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return RedirectToAction("Details", "Issues", new { id = issueId });

        var issue = await _context.Issues.FindAsync(issueId);
        if (issue == null) return NotFound();

        _context.Comments.Add(new Comment
        {
            IssueId = issueId,
            AuthorId = _userManager.GetUserId(User)!,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Issues", new { id = issueId }, "comments");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return BadRequest();

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();
        if (comment.AuthorId != _userManager.GetUserId(User)) return Forbid();

        comment.Content = content.Trim();
        comment.EditedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Issues", new { id = comment.IssueId }, "comments");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();
        if (comment.AuthorId != _userManager.GetUserId(User)) return Forbid();

        var issueId = comment.IssueId;
        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Issues", new { id = issueId }, "comments");
    }
}
