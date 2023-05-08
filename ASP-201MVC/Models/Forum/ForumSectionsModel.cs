namespace ASP_201MVC.Models.Forum
{
	public class ForumSectionsModel
    {
        public ForumThemeFormModel FormModel { get; set; } = null!;
        public String SectionId { get; set; } = null!;
        public List<ForumThemeViewModel> Themes { get; set; } = null!;
        public Boolean UserCanCreate { get; set; }
        public String? CreateMessage { get; set; }
        public bool? IsMessagePositive { get; set; }
    }
}
