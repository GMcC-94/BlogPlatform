using BlogPlatform.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
    }
}
