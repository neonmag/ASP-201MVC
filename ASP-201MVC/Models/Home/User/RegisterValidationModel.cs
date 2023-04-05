namespace ASP_201MVC.Models.Home.User
{
    public class RegisterValidationModel
    {
        public String LoginMessage { get; set; } = null!;
        public String PasswordMessage { get; set; } = null!;
        public String RepeatPasswordMessage { get; set; } = null!;
        public String EmailMessage { get; set; } = null!;
        public String RealNameMessage { get; set; } = null!;
        public IEnumerator<string> GetEnumerator()
        {
            return new List<string> { LoginMessage, PasswordMessage, RepeatPasswordMessage, EmailMessage, RealNameMessage }.GetEnumerator();
        }
    }
}
