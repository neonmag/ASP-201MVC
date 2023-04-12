namespace ASP_201MVC.Services
{
    public class DateService
    {
        public DateTime GetMoment()
        {
            return DateTime.Now.Date;
        }
    }
}
