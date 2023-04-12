namespace ASP_201MVC.Services.Random
{
    public interface IRandomService
    {
        String ConfirmCode(int lenght);
        String RandomString(int length);
        String RandomAvatarName(String fileName, int length);
    }
}
