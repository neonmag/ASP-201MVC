using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using ASP_201MVC.Models.Home.User;
using ASP_201MVC.Services.Hash;
using ASP_201MVC.Services.KDF;
using ASP_201MVC.Data;
using ASP_201MVC.Services.Random;
using ASP_201MVC.Data.Entity;
using Microsoft.Extensions.Primitives;

namespace ASP_201MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHashService _hashService;
        private readonly ILogger<UserController> _logger;
        private readonly DataContext _dataContext;
        private readonly IRandomService _randomService;
        private readonly IKdfService _kdfService;
        public UserController(IHashService hashService, ILogger<UserController> logger, DataContext dataContext, IRandomService randomService, IKdfService kdfService)
        {
            _hashService = hashService;
            _logger = logger;
            _dataContext = dataContext;
            _randomService = randomService;
            _kdfService = kdfService;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Registration()
        {
            return View();
        }
        public IActionResult Register(RegistrationModel registrationModel)
        {
            bool isModelValid = true;
            byte minPasswordLenght = 3;
            RegisterValidationModel registerValidation = new();
            if (String.IsNullOrEmpty(registrationModel.Login))
            {
                registerValidation.LoginMessage = "Login field can't be empty";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.Password))
            {
                registerValidation.PasswordMessage = "Password field can't be empty";
                isModelValid = false;
            }
            else if (registrationModel.Password.Length < minPasswordLenght)
            {
                registerValidation.PasswordMessage = $"Password is too short. At least {minPasswordLenght} symbols";
                registerValidation.RepeatPasswordMessage = "Password field is empty";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.RepeatPassword))
            {
                registerValidation.RepeatPasswordMessage = "Repeat Password field can't be empty";
                isModelValid = false;
            }
            else if (!registrationModel.RepeatPassword.Equals(registrationModel.Password))
            {
                registerValidation.RepeatPasswordMessage = "Repeat Password field isn't match with Password field";
                isModelValid = false;
            }
            if (String.IsNullOrEmpty(registrationModel.Email))
            {
                registerValidation.EmailMessage = "Email field can't be empty";
                isModelValid = false;
            }
            else
            {
                String emailRegex = @"^[\w.%+-]+@[\w.-]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(registrationModel.Email, emailRegex))
                {
                    registerValidation.EmailMessage = "Not valid email";
                }
            }
            if (String.IsNullOrEmpty(registrationModel.RealName))
            {
                registerValidation.RealNameMessage = "Real Name field can't be empty";
                isModelValid = false;
            }
            String savedName = null;
            if (registrationModel.Avatar is not null)
            {
                if (registrationModel.Avatar.Length < 1024)
                {
                    registerValidation.AvatarMessage = "You need image more than 1kb";
                    isModelValid = false;
                }
                else
                {
                    savedName = _randomService.RandomAvatarName(registrationModel.Avatar.FileName, 16);
                    String path = "wwwroot/avatars/" + savedName;
                    FileInfo fileInfo = new FileInfo(path);
                    if (fileInfo.Exists)
                    {
                        do
                        {
                            savedName = _randomService.RandomAvatarName(registrationModel.Avatar.FileName, 16);
                            fileInfo = new FileInfo(path);
                        } while (fileInfo.Exists);
                    }
                    using FileStream fs = new(path, FileMode.Create);
                    registrationModel.Avatar.CopyTo(fs);
                    ViewData["savedName"] = savedName;
                }
            }

            if (isModelValid)
            {
                String salt = _randomService.RandomString(16);
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = registrationModel.Login,
                    RealName = registrationModel.RealName,
                    Email = registrationModel.Email,
                    EmailCode = _randomService.ConfirmCode(6),
                    PasswordSalt = salt,
                    PasswordHash = _kdfService.GetDerivedKey(registrationModel.Password, salt),
                    Avatar = savedName,
                    RegisterDt = DateTime.Now,
                    LastEnter = null
                };
                _dataContext.Users.Add(user);
                _dataContext.SaveChangesAsync();

                return View(registrationModel);
            }
            else
            {
                ViewData["registrationModel"] = registrationModel;
                ViewData["registerValidation"] = registerValidation;
            }

            ViewData["isModelValid"] = isModelValid;
            // способ перейти на View под другим именем
            return View("Registration");
        }
        [HttpPost]
        public String AuthUser()
        {
            // альтернатертивний до моделей спосіб отримання параметрів запиту
            StringValues loginValues = Request.Form["user-login"];
            // колекція loginValues формується при будь-якому ключі, але для
            // неправильних (відсутніх) ключів вона порожня
            if (loginValues.Count == 0)
            {
                // немає логіну у складі полів
                return "Missed required parameter: user-login";
            }
            String login = loginValues[0] ?? "";

            StringValues passwordValues = Request.Form["user-password"];
            // колекція loginValues формується при будь-якому ключі, але для
            // неправильних (відсутніх) ключів вона порожня
            if (passwordValues.Count == 0)
            {
                // немає логіну у складі полів
                return "Missed required parameter: user-password";
            }
            String password = passwordValues[0] ?? "";

            //шукаємо користувача за логіном
            User? user = _dataContext.Users.Where(u => u.Login == login).FirstOrDefault();
            if (user is not null)
            {
                // якщо знайшли - перевіряємо пароль(derived key)
                if (user.PasswordHash == _kdfService.GetDerivedKey(password, user.PasswordSalt))
                {
                    //дані перевірені - користувач автентифікований
                    HttpContext.Session.SetString("authUserId", user.Id.ToString());
                    return "OK";
                }

            }

            return "Авторизацію відхилено";
        }
    }
}
