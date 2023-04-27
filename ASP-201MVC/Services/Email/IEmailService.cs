namespace ASP_201MVC.Services.Email
{
    public interface IEmailService
    {
        bool Send(String mailTemplate, object model);
    }
}
