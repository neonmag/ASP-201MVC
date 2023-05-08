using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using ASP_201MVC.Models.Home.User;
using ASP_201MVC.Services.Hash;
using ASP_201MVC.Services.KDF;
using ASP_201MVC.Data;
using ASP_201MVC.Services.Random;
using ASP_201MVC.Services.Email;
using ASP_201MVC.Data.Entity;
using ASP_201MVC.Services.Validation;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using ASP_201MVC.Models;
using ASP_201MVC.Models.User;
using Microsoft.Identity.Client;

namespace ASP_201MVC.Controllers
{
    public class UserController : Controller
    {
        private readonly IHashService _hashService;
        private readonly ILogger<UserController> _logger;
        private readonly DataContext _dataContext;
        private readonly IRandomService _randomService;
        private readonly IValidationService _validationService;
        private readonly IKdfService _kdfService;
        private readonly IEmailService _emailService;
        public UserController(IHashService hashService, ILogger<UserController> logger, DataContext dataContext, IRandomService randomService, IKdfService kdfService, IValidationService validationService, IEmailService emailService )
        {
            _hashService = hashService;
            _logger = logger;
            _dataContext = dataContext;
            _randomService = randomService;
            _validationService = validationService;
            _kdfService = kdfService;
            _emailService = emailService;
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
            if (_dataContext.Users.Any(u => u.Login == registrationModel.Login))
            {
                registerValidation.PasswordMessage = "Login field can't be not unique (this login is already used)";
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
            if (!_validationService.Validate(registrationModel.Email, ValidationTerms.NotEmpty))
            {
                registerValidation.EmailMessage = "Email field can't be empty";
                isModelValid = false;
            }
            else if (!_validationService.Validate(registrationModel.Email, ValidationTerms.Email))
            {
                registerValidation.EmailMessage = "Not valid email";
                isModelValid = false;
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
                String confirmEmailCode = _randomService.ConfirmCode(6);
                // отправляем код подтверждения
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Login = registrationModel.Login,
                    RealName = registrationModel.RealName,
                    Email = registrationModel.Email,
                    EmailCode = confirmEmailCode,
                    PasswordSalt = salt,
                    PasswordHash = _kdfService.GetDerivedKey(registrationModel.Password, salt),
                    Avatar = savedName,
                    RegisterDt = DateTime.Now,
                    LastEnter = null
                };
                _dataContext.Users.Add(user);

                // Если данные добавленны в БД, отправляем код подверждения на почту
                // генерируем токен автоматческого подтверждения
                var emailConfirmToken = _GenerateEmailConfirmToken(user);

                _dataContext.SaveChangesAsync();

                _SendConfirmEmail(user, emailConfirmToken, "confirm_email");

                _SendConfirmEmail(user, emailConfirmToken, "welcome_email");

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
        private bool _SendConfirmEmail(Data.Entity.User user,
                                        Data.Entity.EmailConfirmToken emailConfirmToken,
                                        String type)
        {
            String confirmLink = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}/User/ConfirmToken?token={emailConfirmToken.Id}";
            return _emailService.Send(type,
                new Models.Email.ConfirmEmailModel
                {
                    Email = user.Email,
                    RealName = user.RealName,
                    EmailCode = user.EmailCode,
                    ConfirmLink = confirmLink
                });
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

            return $"Авторизацію відхилено: Логін:{login}, Пароль:{password}";
        }
        [HttpPatch]
        public String ResendConfirmEmail()
        {
            if(HttpContext.User.Identity?.IsAuthenticated == false)
            {
                return "Unauth";
            }
            try
            {
                User? user = _dataContext.Users.Find(
                    Guid.Parse(
                        HttpContext.User.Claims
                        .First(c => c.Type == ClaimTypes.Sid)
                        .Value)) ?? throw new Exception();
                user.EmailCode = _randomService.ConfirmCode(6);
                var emailConfirmToken = _GenerateEmailConfirmToken(user);
                _dataContext.SaveChangesAsync();
                if (_SendConfirmEmail(user, emailConfirmToken, "confirm_email"))
                    return "ok";
                else
                    return "send error";
            }
            catch
            {
                return "UnAuth";
            }
            return "Ok";
        }

        private EmailConfirmToken _GenerateEmailConfirmToken(Data.Entity.User user)
        {
            Data.Entity.EmailConfirmToken emailConfirmToken = new()
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                UserEmail = user.Email,
                Moment = DateTime.Now,
                Used = 0
            };
            _dataContext.EmailConfirmTokens.Add(emailConfirmToken);
            return emailConfirmToken;
        }
    

        public IActionResult Profile([FromRoute]String id)
        {
            _logger.LogInformation(id);
            User? user = _dataContext.Users.FirstOrDefault(u => u.Login == id);
            if (user is not null)
            {
                ProfileModel model = new(user);
                // достаем ведомости про аутентификацию
                if (HttpContext.User.Identity is not null && HttpContext.User.Identity.IsAuthenticated)
                {
                    String userLogin = HttpContext.User.Claims.First(c => c.Type == ClaimTypes.NameIdentifier)
                    .Value;
                    if (userLogin == user.Login) // Профиль - свой (персональный)
                    {
                        model.IsPersonal = true;
                    }
                }
                _logger.LogInformation("Login: " + model.Login + "\n" + "Email: " + model.Email + "\nIsAuth?" + model.IsEmailConfirmed);
                return View(model);
            }
            else return NotFound();
        }

        [HttpGet]
        public ViewResult ConfirmToken([FromQuery] String token)
        {
            try
            {
                var confirmToken = _dataContext.EmailConfirmTokens
                    .Find(Guid.Parse(token))
                    ?? throw new Exception();
                var user = _dataContext.Users.Find(
                    confirmToken.UserId)
                    ?? throw new Exception();
                if(user.Email != confirmToken.UserEmail)
                {
                    throw new Exception();
                }
                user.EmailCode = null; // Email Confirmed
                confirmToken.Used += 1;
                _dataContext.SaveChangesAsync();
                ViewData["result"] = "Вітаємо, пошта успішно підтверджена";
            }
            catch
            {
                ViewData["result"] = "Перевірка не пройдена, не змінюйте посилання з листа";
            }
            return View();
        }
        public IActionResult Update([FromBody] UpdateRequestModel model)
        {
            UpdateResponseModel responseModel = new();
            try
            {
                if (model is null) throw new Exception("No or empty data");
                if (HttpContext.User.Identity?.IsAuthenticated == false)
                {
                    throw new Exception("UnAuthenticated");
                }
                User? user = _dataContext.Users.Find(
                    Guid.Parse(
                        HttpContext.User.Claims
                        .First(c => c.Type == ClaimTypes.Sid)
                        .Value
                ));
                if (user is null) throw new Exception("UnAuthorized");
                switch (model.Field)
                {
                    case "realname":
                        if (_validationService.Validate(model.Value, ValidationTerms.RealName))
                        {
                            user.RealName = model.Value;
                            _dataContext.SaveChanges();
                        }
                        else throw new Exception(
                                $"Validation error: field '{model.Field}' with value '{model.Value}'");
                        break;
                    case "email":
                        if (_validationService.Validate(model.Value, ValidationTerms.Email))
                        {
                            user.Email = model.Value;
                            _dataContext.SaveChanges();
                        }
                        else throw new Exception(
                                $"Validation error: field '{model.Field}' with value '{model.Value}'");
                        break;
                    default:
                        throw new Exception("Invalid 'Field' attribute");
                }
                responseModel.Status = "OK";
                responseModel.Data = $"Field '{model.Field}' updated by value '{model.Value}'";
            }
            catch (Exception ex)
            {
                responseModel.Status = "Error";
                responseModel.Data = ex.Message;
            }

            return Json(responseModel);

            /* Метод для оновлення даних про користувача
             * Приймає асинхронні запити з JSON даними, повертає JSON
             * із результатом роботи.
             * Приймає дані = описуємо модель цих даних
             * Повертає дані = описуємо модель
             */
        }
        [HttpPost]
        public JsonResult ConfirmEmail([FromBody] string emailCode)
        {
            StatusDataModel model = new();
            if(String.IsNullOrEmpty(emailCode))
            {
                model.Status = "406";
                model.Data = "Empty code not acceptable";
            }
            else if(HttpContext.User.Identity?.IsAuthenticated == false)
            {
                model.Status = "401";
                model.Data = "Unauthenticated";
            }
            else
            {
                User? user = _dataContext.Users.Find(
                    Guid.Parse(HttpContext.User.Claims
                    .First(c => c.Type == ClaimTypes.Sid)
                    .Value));
                if(user is null)
                {
                    model.Status = "403";
                    model.Data = "Forbidden (unAuth)";
                }
                else if(user.EmailCode is null)
                {
                    model.Status = "208";
                    model.Data = "Already Confirmed";
                }
                else if(user.EmailCode != emailCode)
                {
                    model.Status = "406";
                    model.Data = "Code not Accepted";
                }
                else
                {
                    user.EmailCode = null;
                    _dataContext.SaveChanges();
                    model.Status = "200";
                    model.Data = "OK";
                }
            }
            return Json(model);
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("authUserId");
            Response.Redirect("../");
            return RedirectToAction("Index","Home");
        }
    }
}
