using Microsoft.AspNetCore.Mvc;

namespace ASP_201MVC.Models.Forum
{
    public class ForumSectionModel
    {
        [FromForm(Name = "section-title")]
        public String Title { get; set; } = null!;
        [FromForm(Name = "section-description")]
        public String Description { get; set; } = null!;
    }
}
