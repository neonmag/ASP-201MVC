using ASP_201MVC.Services.Hash;

namespace ASP_201MVC.Services.KDF
{
    public class HashKdService : IKdfService
    {
        private readonly IHashService _hashService;

        public HashKdService(IHashService hashService)
        {
            _hashService = hashService;
        }

        public String GetDerivedKey(String password, String salt)
        {
            return _hashService.Hash(salt + password);
        }
    }
}
