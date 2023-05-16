using ASP_201MVC.Data;
using ASP_201MVC.Models;
using ASP_201MVC.Services;
using ASP_201MVC.Services.Hash;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace ASP_201MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DateService _dateService;
        private readonly TimeService _timeService;
        private readonly StampService _stampService;
        private readonly IHashService _hashService;
        private readonly DataContext _dataContext;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, DateService dateServise, TimeService timeServise, StampService stampServise, IHashService hashServise, DataContext dataContext = null, IConfiguration configuration = null)
        {
            _logger = logger;
            _dateService = dateServise;
            _timeService = timeServise;
            _stampService = stampServise;
            _hashService = hashServise;
            _dataContext = dataContext;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Intro()
        {
            return View();
        }

        public IActionResult URL()
        {
            return View();
        }

        public ViewResult WebApi()
        {
            return View();
        }
        public ViewResult Sessions([FromQuery(Name = "session-attr")] String? sessionAttr)
        {
            if (sessionAttr is not null)
            {
                HttpContext.Session.SetString("session-attribute", sessionAttr);
            }
            return View();
        }

        public ViewResult EmailConfirmation()
        {
            ViewData["config"] = _configuration["Smtp:Gmail:Host"];
            return View();
        }

        public IActionResult TagHelpers()
        {
            return View();
        }

        public IActionResult Middleware()
        {
            return View();
        }

        public IActionResult DisplayTemplates()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "DisplayTemplates",
                Title = "Data Display Templates",
                Products = new()
                {
                    new() { Name = "Зарядний кабель",       Image = "1.png", Price = 210 },
                    new() { Name = "Маніпулятор 'миша'",    Image = "2.png", Price = 399.50 },
                    new() { Name = "Наліпка 'Smiley'",      Image = "3.png", Price = 2.95 },
                    new() { Name = "Серветки для монітору", Image = "4.png", Price = 100 },
                    new() { Name = "USB ліхтарик",          Image = "5.png", Price = 49.50 },
                    new() { Name = "Аккумулятор ААА",       Image = "6.png", Price = 280 },
                    new() { Name = "ОС Windows Home",       Image = "7.png", Price = 1250 },
                }
            };
            return View(model);
        }

        public IActionResult PassData()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Models",
                Title = "Data Transfer models",
                Products = new()
                {
                    new() { Name = "Зарядний кабель", Price = 210 },
                    new() { Name = "Маніпулятор 'миша'", Price = 399.50 },
                    new() { Name = "Наліпка 'Smiley'", Price = 2.95 },
                    new() { Name = "Серветки для монітору", Price = 100 },
                    new() { Name = "USB ліхтарик", Price = 49.50 },
                    new() { Name = "Аккумулятор ААА", Price = 280 },
                    new() { Name = "ОС Windows Home", Price = 1250 },
                }
            };
            return View(model);
        }

        public ViewResult Servises()
        {
            ViewData["data_servise"] = _dateService.GetMoment();
            ViewData["data_hashcode"] = _dateService.GetHashCode();

            ViewData["time_servise"] = _timeService.GetMoment();
            ViewData["time_hashcode"] = _timeService.GetHashCode();

            ViewData["stamp_servise"] = _stampService.GetMoment();
            ViewData["stamp_hashcode"] = _stampService.GetHashCode();

            ViewData["hash_service"] = _hashService.Hash("123");
            return View();
        }

        public IActionResult Scheme()
        {
            ViewBag.bagdata = "Data in ViewBag"; // способы передачи данных
            ViewData["data"] = "Data in ViewData"; // к представлению
            return View();
        }

        public IActionResult DataContext()
        {
            ViewData["UsersCount"] = _dataContext.Users.Count();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Razor()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}