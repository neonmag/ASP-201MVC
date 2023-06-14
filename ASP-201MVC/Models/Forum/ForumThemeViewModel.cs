namespace ASP_201MVC.Models.Forum
{
    public class ForumThemeViewModel
    {
        public String Title { get; set; } = null!;
        public String Description { get; set; } = null!;
        public String UrlIdString { get; set; } = null!;
        public String SectionId { get; set; } = null!;
        public String CreatedDtString { get; set; } = null!;


        public String AuthorName { get; set; } = null!;
        public String AuthorAvatarUrl { get; set; } = null!;
        public DateTime ProfileCreateDt { get; set; }

        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int? GivenRating { get; set; }
    }
}
