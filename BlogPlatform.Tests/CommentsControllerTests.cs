using BlogPlatform.Controllers;
using BlogPlatform.Data;
using BlogPlatform.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BlogPlatform.Tests
{
    public class CommentsControllerTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private CommentsController CreateControllerWithUser(ApplicationDbContext context, string userName, string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));

            var controller = new CommentsController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };

            return controller;
        }

        /*
         * @params int postId - the ID of the post to comment on
         *         string body - valid comment text
         * @returns RedirectToActionResult to Posts/Details and comment is saved to the database
         */
        [Fact]
        public async Task Create_SavesCommentAndRedirectsToDetails_WhenValid()
        {
            using var context = GetInMemoryContext();
            var author = new IdentityUser { Id = "user1", UserName = "testuser" };
            context.Users.Add(author);
            context.Posts.Add(new Post
            {
                Id = 1,
                Title = "Test Post",
                Body = "Test Body",
                CreatedAt = DateTime.UtcNow,
                AuthorId = "user1"
            });
            await context.SaveChangesAsync();
            var controller = CreateControllerWithUser(context, "testuser", "user1");
            var result = await controller.Create(1, "Test Comment");
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Posts", redirect.ControllerName);
            var savedComment = await context.Comments.FirstOrDefaultAsync(c => c.Body == "Test Comment");
            Assert.NotNull(savedComment);
            Assert.Equal("user1", savedComment.AuthorId);
            Assert.Equal(1, savedComment.PostId);
        }

        /*
         * @params int postId - an ID that does not exist in the database
         *         string body - valid comment text
         * @returns NotFoundResult when post does not exist
         */
        [Fact]
        public async Task Create_ReturnsNotFound_WhenPostDoesNotExist()
        {
            using var context = GetInMemoryContext();
            var controller = CreateControllerWithUser(context, "testuser", "user1");
            var result = await controller.Create(999, "Test Comment");
            Assert.IsType<NotFoundResult>(result);
        }

        /*
         * @params int postId - the ID of the post to comment on
         *         string body - empty string
         * @returns RedirectToActionResult to Posts/Details without saving when body is empty
         */
        [Fact]
        public async Task Create_RedirectsToDetails_WithoutSaving_WhenBodyIsEmpty()
        {
            using var context = GetInMemoryContext();
            var author = new IdentityUser { Id = "user1", UserName = "testuser" };
            context.Users.Add(author);
            context.Posts.Add(new Post
            {
                Id = 1,
                Title = "Test Post",
                Body = "Test Body",
                CreatedAt = DateTime.UtcNow,
                AuthorId = "user1"
            });
            await context.SaveChangesAsync();
            var controller = CreateControllerWithUser(context, "testuser", "user1");
            var result = await controller.Create(1, "");
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal("Posts", redirect.ControllerName);
            var commentCount = await context.Comments.CountAsync();
            Assert.Equal(0, commentCount);
        }
    }
}