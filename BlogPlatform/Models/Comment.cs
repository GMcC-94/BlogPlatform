using Microsoft.AspNetCore.Identity;

namespace BlogPlatform.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }
        public string AuthorId { get; set; }
        public IdentityUser Author { get; set; }
        public int PostId { get; set; }
        public Post Post { get; set; }
    }
}
