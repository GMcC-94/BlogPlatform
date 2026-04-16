using BlogPlatform.Data;
using BlogPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogPlatform.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*
         * @params int postId - the ID of the post to comment on
         *         string body - the comment text
         * @returns RedirectToActionResult to Posts/Details on success,
         *          NotFoundResult if the post does not exist,
         *          redirects to Posts/Details without saving if body is empty,
         *          redirects to login if user is not authenticated
         */
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int postId, string body)
        {
            var post = await _context.Posts.FindAsync(postId);
            if (post == null) return NotFound();

            if (string.IsNullOrWhiteSpace(body))
                return RedirectToAction("Details", "Posts", new { id = postId });

            var comment = new Comment
            {
                Body = body,
                CreatedAt = DateTime.UtcNow,
                PostId = postId,
                AuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier)!
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new { id = postId });
        }
    }
}
