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

        public HomeController(ILogger<HomeController> logger, DateService dateService,
                                                               TimeService timeService,
                                                               StampService stampService,
                                                               IHashService hashService,
                                                               DataContext dataContext)
        {
            _logger = logger;
            _dateService = dateService;
            _timeService = timeService;
            _stampService = stampService;
            _hashService = hashService;
            _dataContext = dataContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult ContextD()
        {
            ViewData["UsersCount"] = _dataContext.Users.Count();
            return View("ContextD");
        }
        public ViewResult Context()
        {
            ViewData["UsersCount"] = _dataContext.Users.Count();
            return View("Context");
        }
        public ViewResult Services()
        {
            ViewData["date_service"] = _dateService.GetMoment();
            ViewData["date_hashcode"] = _dateService.GetHashCode();

            ViewData["time_service"] = _timeService.GetMoment();
            ViewData["time_hashcode"] = _timeService.GetHashCode();

            ViewData["stamp_service"] = _stampService.GetMoment();
            ViewData["stamp_hashcode"] = _stampService.GetHashCode();
            return View();
        }

        public IActionResult Intro()
        {
            return View();
        }
        public IActionResult Scheme()
        {
            ViewBag.bagdata = "Data in ViewBag";     // Способи передачі даних
            ViewData["data"] = "Data in ViewData";   // до представлення

            return View();
        }
        public IActionResult Razor()
        {
            return View();
        }
        public ViewResult Middleware()
        {
            return View();
        }
        public IActionResult Sessions([FromQuery(Name = "session-attr")]String? sessionsAttr)
        {
            if(sessionsAttr is not null)
            {
                HttpContext.Session.SetString("session-attribute", sessionsAttr);
            }
            return View();
        }
        public IActionResult FirstHw()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Перше дз",
                Title = "ПЕРШЕ ДЗ",
                Products = new()
                {
                    new() {Name = "Кабель", Price = 20,            Image="img1.png"         },
                    new() {Name = "Миша", Price = 420,             Image="img2.png"         },
                    new() {Name = "Серветки", Price = 14,          Image="img3.png"         },
                    new() {Name = "Наліпка", Price = 45,           Image="img1.png"         },
                    new() {Name = "USB Ліхтарик", Price = 2100.22, Image="img4.jpg"         },
                    new() {Name = "Акумулятор ААА", Price = 54320, Image="img2.png"         },
                    new() {Name = "OC Windows Home", Price = 7.50, Image="img3.png"         }
                }
            };
            return View(model);
        }
        public IActionResult URL()
        {
            ViewBag.bagdata = "Data in ViewBag";     // Способи передачі даних
            ViewData["data"] = "Data in ViewData";   // до представлення
            return View();
        }
        public IActionResult PassData()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Моделі",
                Title = "Моделі передачі даних",
                Products = new()
                {
                    new() {Name = "Кабель", Price = 20,            Image="img1.png"         },
                    new() {Name = "Миша", Price = 420,             Image="img2.png"         },
                    new() {Name = "Серветки", Price = 14,          Image="img3.png"         },
                    new() {Name = "Наліпка", Price = 45,           Image="img1.png"         },
                    new() {Name = "USB Ліхтарик", Price = 2100.22, Image="img4.jpg"         },
                    new() {Name = "Акумулятор ААА", Price = 54320, Image="img2.png"         },
                    new() {Name = "OC Windows Home", Price = 7.50, Image="img3.png"         }
                }
            };
            return View(model);
        }
        public IActionResult DisplayTemplates()
        {
            Models.Home.PassDataModel model = new()
            {
                Header = "Шаблони",
                Title = "Шаблони відображення даних",
                Products = new()
                {
                    new() {Name = "Кабель", Price = 20,            Image="img1.png"         },
                    new() {Name = "Миша", Price = 420,             Image="img2.png"         },
                    new() {Name = "Серветки", Price = 14,          Image="img3.png"         },
                    new() {Name = "Наліпка", Price = 45,           Image="img1.png"         },
                    new() {Name = "USB Ліхтарик", Price = 2100.22, Image="img4.jpg"         },
                    new() {Name = "Акумулятор ААА", Price = 54320, Image="img2.png"         },
                    new() {Name = "OC Windows Home", Price = 7.50, Image="img3.png"         }
                }
            };
            return View(model);
        }
        public IActionResult Privacy()
        {
            return null;
        }
        public IActionResult TagHelper()
        {
            return View();
        }
        public IActionResult SecondHw()
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