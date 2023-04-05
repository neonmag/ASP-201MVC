using ASP_201MVC.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Reflection;

namespace ASP_201MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
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