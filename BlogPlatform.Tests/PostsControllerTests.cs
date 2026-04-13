using BlogPlatform.Controllers;
using BlogPlatform.Data;
using BlogPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [Fact]
        public async Task Index_ReturnsViewResult_WithPostsOrderedByDateDescending()
        {
            // Arrange
            using var context = GetInMemoryContext();

            var author = new IdentityUser { Id = "user1", UserName = "testuser" };
            context.Users.Add(author);

            context.Posts.AddRange(
                new Post { Id = 1, Title = "First", Body = "Body 1", CreatedAt = DateTime.UtcNow.AddDays(-2), AuthorId = "user1" },
                new Post { Id = 2, Title = "Second", Body = "Body 2", CreatedAt = DateTime.UtcNow.AddDays(-1), AuthorId = "user1" },
                new Post { Id = 3, Title = "Third", Body = "Body 3", CreatedAt = DateTime.UtcNow, AuthorId = "user1" }
            );
            await context.SaveChangesAsync();

            var controller = new PostsController(context);

            // Act
            var result = await controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Post>>(viewResult.Model);
            Assert.Equal(3, model.Count);
            Assert.Equal("Third", model[0].Title);
            Assert.Equal("First", model[2].Title);
        }
    }
}
