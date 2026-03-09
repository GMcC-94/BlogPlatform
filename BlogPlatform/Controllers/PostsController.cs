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

        public async Task<IActionResult> Index()
        {
            var posts = await _context.Posts
                .Include(p => p.Author)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();


            return View(posts); ;
        }
    }
}
