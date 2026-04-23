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
    public class PostsControllerTests
    {
        private ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private PostsController CreateControllerWithUser(ApplicationDbContext context, string userName, string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier, userId)
            }, "mock"));

            var controller = new PostsController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };

            return controller;
        }

        /*
         * @params none
         * @returns ViewResult with a list of all posts ordered by date descending
         */
        [Fact]
        public async Task Index_ReturnsViewResult_WithPostsOrderedByDateDescending()
        {
            
            using var context = GetInMemoryContext();

            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
            context.Users.Add(author);

            context.Posts.AddRange(
                new Post { Id = 1, Title = "First", Body = "Body 1", CreatedAt = DateTime.UtcNow.AddDays(-2), AuthorId = "user1" },
                new Post { Id = 2, Title = "Second", Body = "Body 2", CreatedAt = DateTime.UtcNow.AddDays(-1), AuthorId = "user1" },
                new Post { Id = 3, Title = "Third", Body = "Body 3", CreatedAt = DateTime.UtcNow, AuthorId = "user1" }
            );
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context, "testuser", "user1");

            
            var result = await controller.Index();

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Post>>(viewResult.Model);
            Assert.Equal(3, model.Count);
            Assert.Equal("Third", model[0].Title);
            Assert.Equal("First", model[2].Title);
        }

        /*
         * @params int id - the ID of the post to retrieve
         * @returns ViewResult with the correct Post data when post exists
         */
        [Fact]
        public async Task Details_ReturnsViewResult_WithPost_WhenPostExists()
        {
            
            using var context = GetInMemoryContext();

            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
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

            var controller = new PostsController(context);

           
            var result = await controller.Details(1);

            
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Post>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Test Post", model.Title);
        }

        /*
         * @params int id - an ID that does not exist in the database
         * @returns NotFoundResult when post does not exist
         */
        [Fact]
        public async Task Details_ReturnsNotFound_WhenPostDoesNotExist()
        {
            
            using var context = GetInMemoryContext();
            var controller = new PostsController(context);

            
            var result = await controller.Details(999);

            
            Assert.IsType<NotFoundResult>(result);
        }

        /*
         * @params none
         * @returns ViewResult for the Create form
         */
        [Fact]
        public void Create_Get_ReturnsViewResult()
        {
            
            using var context = GetInMemoryContext();
            var controller = new PostsController(context);

           
            var result = controller.Create();

            
            Assert.IsType<ViewResult>(result);
        }


        /*
         * @params string title - empty string
         *         string body - empty string
         * @returns ViewResult with model error when title or body are empty
         */
        [Fact]
        public async Task Create_Post_ReturnsViewResult_WhenTitleOrBodyIsEmpty()
        {
            
            using var context = GetInMemoryContext();
            var controller = new PostsController(context);

            
            var result = await controller.Create("", "");

            
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }
            
        /*
         * @params int id - the ID of the post to edit
         * @returns ViewResult with the Post model when the current user is the author
         */
        [Fact]
        public async Task Edit_Get_ReturnsViewResult_WhenUserIsAuthor()
        {
            
            using var context = GetInMemoryContext();

            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
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

           
            var result = await controller.Edit(1);

           
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Post>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Test Post", model.Title);
        }

        /*
         * @params int id - the ID of a post that does not exist
         * @returns NotFoundResult when post does not exist
         */
        [Fact]
        public async Task Edit_Get_ReturnsNotFound_WhenPostDoesNotExist()
        {
           
            using var context = GetInMemoryContext();
            var controller = CreateControllerWithUser(context, "testuser", "user1");

            
            var result = await controller.Edit(999);

           
            Assert.IsType<NotFoundResult>(result);
        }

        /*
         * @params int id - the ID of a post owned by a different user
         * @returns ForbidResult when the current user is not the author
         */
        [Fact]
        public async Task Edit_Get_ReturnsForbid_WhenUserIsNotAuthor()
        {
            
            using var context = GetInMemoryContext();

            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
            var otherUser = new ApplicationUser { Id = "user2", UserName = "otheruser" };
            context.Users.AddRange(author, otherUser);
            context.Posts.Add(new Post
            {
                Id = 1,
                Title = "Test Post",
                Body = "Test Body",
                CreatedAt = DateTime.UtcNow,
                AuthorId = "user1"
            });
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context, "otheruser", "user2");

            
            var result = await controller.Edit(1);

            
            Assert.IsType<ForbidResult>(result);
        }

        /*
         * @params int id - the ID of the post to edit
         *         string title - updated title
         *         string body - updated body
         * @returns RedirectToActionResult to Index and post is updated in the database
         */
        [Fact]
        public async Task Edit_Post_UpdatesPostAndRedirectsToIndex_WhenValid()
        {
            
            using var context = GetInMemoryContext();

            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
            context.Users.Add(author);
            context.Posts.Add(new Post
            {
                Id = 1,
                Title = "Old Title",
                Body = "Old Body",
                CreatedAt = DateTime.UtcNow,
                AuthorId = "user1"
            });
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context, "testuser", "user1");

            
            var result = await controller.Edit(1, "New Title", "New Body");

            
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            var updatedPost = await context.Posts.FindAsync(1);
            Assert.Equal("New Title", updatedPost!.Title);
            Assert.Equal("New Body", updatedPost.Body);
        }

        /*
         * @params int id - the ID of the post to edit
         *         string title - empty string
         *         string body - empty string
         * @returns ViewResult with model error when title or body are empty
         */
        [Fact]
        public async Task Edit_Post_ReturnsViewResult_WhenTitleOrBodyIsEmpty()
        {
            
            using var context = GetInMemoryContext();

            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
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

           
            var result = await controller.Edit(1, "", "");

          
            Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        /*
         * @params int id - the ID of a post owned by a different user
         * @returns ForbidResult when the current user is not the author
         */
        [Fact]
        public async Task Edit_Post_ReturnsForbid_WhenUserIsNotAuthor()
        {
            
            using var context = GetInMemoryContext();

            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
            var otherUser = new ApplicationUser { Id = "user2", UserName = "otheruser" };
            context.Users.AddRange(author, otherUser);
            context.Posts.Add(new Post
            {
                Id = 1,
                Title = "Test Post",
                Body = "Test Body",
                CreatedAt = DateTime.UtcNow,
                AuthorId = "user1"
            });
            await context.SaveChangesAsync();

            var controller = CreateControllerWithUser(context, "otheruser", "user2");

            
            var result = await controller.Edit(1, "New Title", "New Body");

            
            Assert.IsType<ForbidResult>(result);
        }

        /*
         * @params int id - the ID of the post to delete
         * @returns ViewResult with the Post model when the current user is the author
         */
        [Fact]
        public async Task Delete_Get_ReturnsViewResult_WhenUserIsAuthor()
        {
            using var context = GetInMemoryContext();
            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
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
            var result = await controller.Delete(1);
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Post>(viewResult.Model);
            Assert.Equal(1, model.Id);
            Assert.Equal("Test Post", model.Title);
        }

        /*
         * @params int id - an ID that does not exist in the database
         * @returns NotFoundResult when post does not exist
         */
        [Fact]
        public async Task Delete_Get_ReturnsNotFound_WhenPostDoesNotExist()
        {
            using var context = GetInMemoryContext();
            var controller = CreateControllerWithUser(context, "testuser", "user1");
            var result = await controller.Delete(999);
            Assert.IsType<NotFoundResult>(result);
        }

        /*
         * @params int id - the ID of a post owned by a different user
         * @returns ForbidResult when the current user is not the author
         */
        [Fact]
        public async Task Delete_Get_ReturnsForbid_WhenUserIsNotAuthor()
        {
            using var context = GetInMemoryContext();
            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
            var otherUser = new ApplicationUser { Id = "user2", UserName = "otheruser" };
            context.Users.AddRange(author, otherUser);
            context.Posts.Add(new Post
            {
                Id = 1,
                Title = "Test Post",
                Body = "Test Body",
                CreatedAt = DateTime.UtcNow,
                AuthorId = "user1"
            });
            await context.SaveChangesAsync();
            var controller = CreateControllerWithUser(context, "otheruser", "user2");
            var result = await controller.Delete(1);
            Assert.IsType<ForbidResult>(result);
        }

        /*
         * @params int id - the ID of the post to delete
         * @returns RedirectToActionResult to Index and post is removed from the database
         */
        [Fact]
        public async Task DeleteConfirmed_DeletesPostAndRedirectsToIndex_WhenUserIsAuthor()
        {
            using var context = GetInMemoryContext();
            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
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
            var result = await controller.DeleteConfirmed(1);
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            var deletedPost = await context.Posts.FindAsync(1);
            Assert.Null(deletedPost);
        }

        /*
         * @params int id - an ID that does not exist in the database
         * @returns NotFoundResult when post does not exist
         */
        [Fact]
        public async Task DeleteConfirmed_ReturnsNotFound_WhenPostDoesNotExist()
        {
            using var context = GetInMemoryContext();
            var controller = CreateControllerWithUser(context, "testuser", "user1");
            var result = await controller.DeleteConfirmed(999);
            Assert.IsType<NotFoundResult>(result);
        }

        /*
         * @params int id - the ID of a post owned by a different user
         * @returns ForbidResult when the current user is not the author
         */
        [Fact]
        public async Task DeleteConfirmed_ReturnsForbid_WhenUserIsNotAuthor()
        {
            using var context = GetInMemoryContext();
            var author = new ApplicationUser { Id = "user1", UserName = "testuser" };
            var otherUser = new ApplicationUser { Id = "user2", UserName = "otheruser" };
            context.Users.AddRange(author, otherUser);
            context.Posts.Add(new Post
            {
                Id = 1,
                Title = "Test Post",
                Body = "Test Body",
                CreatedAt = DateTime.UtcNow,
                AuthorId = "user1"
            });
            await context.SaveChangesAsync();
            var controller = CreateControllerWithUser(context, "otheruser", "user2");
            var result = await controller.DeleteConfirmed(1);
            Assert.IsType<ForbidResult>(result);
        }
    }

}
