namespace ASP_201MVC.Services.KDF
{
    public interface IKdfService
    {
        String GetDerivedKey(String password, String salt);
    }
}
