namespace ASP_201MVC.Models.Forum
{
	public class ForumThemesModel
    {
        public ForumTopicFormModel FormModel { get; set; } = null!;
        public String ThemeId { get; set; } = null!;
        public String Title { get; set; } = null!;
        public List<ForumTopicViewModel> Topics { get; set; } = null!;
        public Boolean UserCanCreate { get; set; }
        public bool? IsMessagePositive { get; set; }
        public String? CreateMessage { get; set; }
    }
}
