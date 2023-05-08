using Microsoft.AspNetCore.Mvc;
using ASP_201MVC.Models.Forum;
using ASP_201MVC.Data;
using ASP_201MVC.Services.Validation;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace ASP_201MVC.Controllers
{
    public class ForumController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ForumController> _logger;
        private readonly IValidationService _validationService;
        public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService)
        {
            _dataContext = dataContext;
            _logger = logger;
            _validationService = validationService;
        }
        private int _counter = 0;

        private int Counter { get => _counter++; set => _counter = value; }
        private void CheckInvalidForm(string errorMessage, string savedTitle, string savedDescription)
        {
            HttpContext.Session.SetInt32("IsMessagePositive", 0);
            HttpContext.Session.SetString("CreateSectionMessage", errorMessage);
            HttpContext.Session.SetString("SavedTitle", savedTitle ?? String.Empty);
            HttpContext.Session.SetString("SavedDescription", savedDescription ?? String.Empty);
        }
        public IActionResult Index()
        {
            ForumIndexModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Sections = _dataContext.Sections
                .Include(s => s.Author)
                .Where(s => s.Deleted == null).
                OrderBy(s => s.CreatedDt)
                .AsEnumerable()
                .Select(s => new ForumSectionViewModel()
                {
                    Title = s.Title,
                    Description = s.Description,
                    LogoUrl = $"/img/logos/section{Counter}.png",
                    UrlIdString = s.Id.ToString(),
                    CreatedDt = DateTime.Today == s.CreatedDt.Date ?
                    "Сьогодні: " + s.CreatedDt.ToString("HH:mm")
                    : s.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    AuthorName = s.Author.IsRealNamePublic ? s.Author.RealName : s.Author.Login,
                    AuthorAvatarUrl = s.Author.Avatar == null
                    ? "/avatars/no-avatar.png"
                    : $"/avatars/{s.Author.Avatar}"
                }).ToList()
            };


            if (HttpContext.Session.GetString("CreateSectionMessage") is String message)
            {
                HttpContext.Session.Remove("CreateSectionMessage");
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if (model.IsMessagePositive == false)
                {
                    model.FormModel = new()
                    {
                        Title = HttpContext.Session.GetString("SavedTitle"),
                        Description = HttpContext.Session.GetString("SavedDescription")
                    };
                    HttpContext.Session.Remove("SavedTitle");
                    HttpContext.Session.Remove("SavedDescription");
                }
                HttpContext.Session.Remove("CreateSessionMessage");
                HttpContext.Session.Remove("IsMessagePositive");
            }
            return View(model);
        }
        [HttpPost]
        public RedirectToActionResult CreateSection(ForumSectionModel formModel)
        {
            _logger.LogInformation("Title: {t}, Description: {d}",
                formModel.Title, formModel.Description);

            if (!_validationService.Validate(formModel.Title, ValidationTerms.NotEmpty))
            {
                CheckInvalidForm("Name can't be empty", formModel.Title, formModel.Description);
            }
            else if (!_validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                CheckInvalidForm("Description can't be empty", formModel.Title, formModel.Description);
            }
            else
            {
                try
                {
                    Guid userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
                    _dataContext.Sections.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);
                    HttpContext.Session.SetString("CreateSectionMessage",
                        "Added successfully");
                }
                catch
                {
                    CheckInvalidForm("Authorization denied", formModel.Title, formModel.Description);
                }

            }
            return RedirectToAction(nameof(Index));
        }
        public ViewResult Sections([FromRoute] String id)
        {
            ForumSectionsModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                SectionId = id,
                Themes = _dataContext
                .Themes.Where(t => t.Deleted == null && t.SectionId == Guid.Parse(id))
                .Select(t => new ForumThemeViewModel()
                {
                    Title = t.Title,
                    Description = t.Description,
                    CreatedDtString = DateTime.Today == t.CreatedDt.Date
                    ? "Сьогодні " + t.CreatedDt.ToString("HH:mm")
                    : t.CreatedDt.ToString("dd.MM.yyy HH:mm"),
                    UrlIdString = t.Id.ToString(),
                    SectionId = t.SectionId.ToString()
                }).ToList()
            };
            if (HttpContext.Session.GetString("CreateSectionMessage") is String message)
            {
                HttpContext.Session.Remove("CreateSectionMessage");
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if (model.IsMessagePositive == false)
                {
                    model.FormModel = new()
                    {
                        Title = HttpContext.Session.GetString("SavedTitle")!,
                        Description = HttpContext.Session.GetString("SavedDescription")!
                    };
                    HttpContext.Session.Remove("SavedTitle");
                    HttpContext.Session.Remove("SavedDescription");
                }
                HttpContext.Session.Remove("CreateSessionMessage");
                HttpContext.Session.Remove("IsMessagePositive");
            }
            return View(model);
        }
        [HttpPost]
        public RedirectToActionResult CreateTheme(ForumThemeFormModel formModel)
        {
            if (!_validationService.Validate(formModel.Title, ValidationTerms.NotEmpty))
            {
                CheckInvalidForm("Name can't be empty", formModel.Title, formModel.Description);
            }
            else if (!_validationService.Validate(formModel.Description, ValidationTerms.NotEmpty))
            {
                CheckInvalidForm("Description can't be empty", formModel.Title, formModel.Description);
            }
            else
            {
                Guid userId;
                try
                {
                    userId = Guid.Parse(
                       HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
                    _dataContext.Themes.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        SectionId = Guid.Parse(formModel.SectionId)
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetString("CreateSectionMessage", "ALL OK");
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);
                }
                catch
                {
                    CheckInvalidForm("Authorization denied", formModel.Title, formModel.Description);
                }
            }

            return RedirectToAction(nameof(Sections), new { id = formModel.SectionId });
        }

    }
}