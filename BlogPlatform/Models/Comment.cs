using Microsoft.AspNetCore.Identity;

namespace BlogPlatform.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string AuthorId { get; set; } = string.Empty;
        public IdentityUser Author { get; set; } = null!;
        public int PostId { get; set; }
        public Post BlogPost { get; set; } = null!;
    }
}