using BlogPlatform.Data;
using BlogPlatform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogPlatform.Controllers
{
    public class PostsController : Controller 
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /*
         * @params none
         * @returns ViewResult with a list of all posts ordered by date descending,
         *          including each post's author
         */
        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();


            return View(posts); ;
        }

        /*
         * @params int id - the ID of the post to retrieve
         * @returns ViewResult with the Post model including author and comments,
         *          or NotFoundResult if the post does not exist
         */
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();

            return View(post);
        }

        /*
         * @params none
         * @returns ViewResult for the Create form,
         *          redirects to login if user is not authenticated
         */
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        /*
         * @params string title - the title of the post
         *         string body - the body content of the post
         * @returns RedirectToActionResult to Index on success,
         *          ViewResult with validation errors if title or body are empty,
         *          redirects to login if user is not authenticated
         */
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string title, string body)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
            {
                ModelState.AddModelError("", "Title and body are required.");
                return View();
            }

            var post = new Post
            {
                Title = title,
                Body = body,
                CreatedAt = DateTime.UtcNow,
                AuthorId = _context.Users.First(u => u.UserName == User.Identity!.Name).Id
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            if (post.AuthorId != GetCurrentUserId()) return Forbid();

            return View(post);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string title, string body)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null) return NotFound();
            if (post.AuthorId != GetCurrentUserId()) return Forbid();

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
            {
                ModelState.AddModelError("", "Title and body are required.");
                return View(post);
            }

            post.Title = title;
            post.Body = body;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /*
         * @params int id - the ID of the post to delete
         * @returns ViewResult with the Post model if the current user is the author,
         *          NotFoundResult if the post does not exist,
         *          ForbidResult if the current user is not the author
         */
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null) return NotFound();
            if (post.AuthorId != GetCurrentUserId()) return Forbid();

            return View(post);
        }

        /*
         * @params int id - the ID of the post to delete
         * @returns RedirectToActionResult to Index on success,
         *          NotFoundResult if the post does not exist,
         *          ForbidResult if the current user is not the author
         */
        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);

            if (post == null) return NotFound();
            if (post.AuthorId != GetCurrentUserId()) return Forbid();

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        /*
        * Helper method for getting the current user ID 
        */
        private string GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        }
    }
}
