namespace ASP_201MVC.Models.Email
{
    public class ConfirmEmailModel
    {
        public String RealName { get; set; } = null!;
        public String Email { get; set; } = null!;
        public String EmailCode { get; set; } = null!;
        public String ConfirmLink { get; set; } = null!;
    }
}
