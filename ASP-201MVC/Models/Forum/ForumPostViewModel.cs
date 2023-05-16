using Microsoft.Identity.Client;

namespace ASP_201MVC.Models.Forum
{
	public class ForumPostViewModel
	{
        public String Content { get; set; } = null!;
        public String AuthorName { get; set; } = null!;
        public String AuthorAvatarUrl { get; set; } = null!;
    }
}
