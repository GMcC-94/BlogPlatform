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


        /*
          * @params int id - the ID of the comment to edit
         *         string body - updated comment text
         * @returns RedirectToActionResult to Posts/Details on success,
         *          NotFoundResult if comment does not exist,
         *          ForbidResult if current user is not the author
         */
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string body)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();
            if (comment.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();

            if (string.IsNullOrWhiteSpace(body))
                return RedirectToAction("Details", "Posts", new { id = comment.PostId });

            comment.Body = body;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new { id = comment.PostId });
        }

        /*
         * @params int id - the ID of the comment to delete
         * @returns RedirectToActionResult to Posts/Details on success,
         *          NotFoundResult if comment does not exist,
         *          ForbidResult if current user is not the author
         */
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();
            if (comment.AuthorId != User.FindFirstValue(ClaimTypes.NameIdentifier)) return Forbid();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Posts", new { id = comment.PostId });
        }
    }
}
