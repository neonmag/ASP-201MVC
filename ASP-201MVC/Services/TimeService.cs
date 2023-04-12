namespace ASP_201MVC.Services
{
    public class TimeService
    {
        public DateTime GetMoment()
        {
            return DateTime.Now.ToLocalTime();
        }
    }
}
