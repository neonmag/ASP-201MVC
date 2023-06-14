using Microsoft.AspNetCore.Mvc;
using ASP_201MVC.Models.Forum;
using ASP_201MVC.Data;
using ASP_201MVC.Services.Validation;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ASP_201MVC.Services.Transliterate;
using System.Reflection.Metadata.Ecma335;
using ASP_201MVC.Migrations;
using System.ComponentModel;

namespace ASP_201MVC.Controllers
{
    public class ForumController : Controller
    {
        private readonly DataContext _dataContext;
        private readonly ILogger<ForumController> _logger;
        private readonly IValidationService _validationService;
        private readonly ITransliterationService _transliterationService;
        public ForumController(DataContext dataContext, ILogger<ForumController> logger, IValidationService validationService, ITransliterationService transliterationService)
        {
            _dataContext = dataContext;
            _logger = logger;
            _validationService = validationService;
            _transliterationService = transliterationService;
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
            Counter = 0;
            String? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;
            ForumIndexModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Sections = _dataContext.Sections
                .Include(s => s.Author)
                .Include(s => s.RateList)
                .Where(s => s.Deleted == null).
                OrderBy(s => s.CreatedDt)
                .AsEnumerable()
                .Select(s => new ForumSectionViewModel()
                {
                    Title = s.Title,
                    Description = s.Description,
                    IdString = s.Id.ToString(),
                    LogoUrl = $"/img/logos/section{Counter}.png",
                    CreatedDt = DateTime.Today == s.CreatedDt.Date
                    ?
                    "Сьогодні: " + s.CreatedDt.ToString("HH:mm")
                    : s.CreatedDt.ToString("dd.MM.yyyy HH:mm"),
                    UrlIdString = s.UrlId is null ? s.Id.ToString() : s.UrlId,
                    AuthorName = s.Author.IsRealNamePublic ? s.Author.RealName : s.Author.Login,
                    AuthorAvatarUrl = s.Author.Avatar == null
                    ? "/avatars/no-avatar.png"
                    : $"/avatars/{s.Author.Avatar}",
                    LikesCount = s.RateList.Count(r => r.Rating > 0),
                    DislikesCount = s.RateList.Count(r => r.Rating < 0),
                    GivenRating = userId == null? null
                    : s.RateList.FirstOrDefault(r => r.UserId == Guid.Parse(userId))?.Rating
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
                    String urlId = _transliterationService.Transliterate(formModel.Title);
                    int n = 2;
                    while (_dataContext.Sections.Where(s => s.UrlId == urlId).Count() > 0)
                    {
                        urlId = $"trans{n++}";
                    }
                    _dataContext.Sections.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        UrlId = urlId
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
            Guid sectionId;
            try
            {
                sectionId = Guid.Parse(id);
            }
            catch
            {
                sectionId = _dataContext.Sections.First(s => s.UrlId == id).Id;
            }
            String? userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value;

            ForumSectionsModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                SectionId = sectionId.ToString(),
                Themes = _dataContext
                .Themes
                .Include(t => t.Author)
                .Include(t => t.RateList)
                .Where(t => t.Deleted == null && t.SectionId == sectionId)
                .AsEnumerable()
                .Select(t => new ForumThemeViewModel()
                {
                    Title = t.Title,
                    Description = t.Description,
                    CreatedDtString = DateTime.Today == t.CreatedDt.Date
                    ? "Сьогодні " + t.CreatedDt.ToString("HH:mm")
                    : t.CreatedDt.ToString("dd.MM.yyy HH:mm"),
                    UrlIdString = t.Id.ToString(),
                    SectionId = t.SectionId.ToString(),
                    ProfileCreateDt = t.CreatedDt,
                    AuthorName = t.Author.IsRealNamePublic
                                   ? t.Author.RealName
                                   : t.Author.Login,
                    AuthorAvatarUrl = $"/avatars/{t.Author.Avatar ?? "no-avatar.png"}",
                    LikesCount = t.RateList.Count(r => r.Rating > 0),
                    DislikesCount = t.RateList.Count(r => r.Rating < 0),
                    GivenRating = userId == null ? null
                    : t.RateList.FirstOrDefault(r => r.UserId == Guid.Parse(userId))?.Rating
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
            _logger.LogInformation("Linchevsky id: " + id);
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

            return RedirectToAction(nameof(Sections),
                new { id = formModel.SectionId });
        }
        public IActionResult Topics([FromRoute] String id)
        {
            Guid topicId;
            try
            {
                topicId = Guid.Parse(id);
            }
            catch
            {
                topicId = Guid.Empty;
            }
            var topic = _dataContext.Themes.Find(topicId);
            if (topic == null)
            {
                return NotFound();
            }
            ForumTopicsModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Title = topic.Title,
                Description = topic.Description,
                TopicId = id,
                Posts = _dataContext
                .Posts
                .Where(p => p.Deleted == null && p.TopicId == topicId)
                .Select(p => new ForumPostViewModel
                {
                    Content = p.Content
                })
                .ToList()

            };
            if (HttpContext.Session.GetString("CreateSectionMessage") is String message)
            {
                model.CreateMessage = message;
                model.IsMessagePositive = HttpContext.Session.GetInt32("IsMessagePositive") == 1;
                if (model.IsMessagePositive == false)
                {
                    model.FormModel = new()
                    {
                        Content = HttpContext.Session.GetString("SavedContent")!,
                        ReplyId = HttpContext.Session.GetString("SavedReplyId")!
                    };
                    HttpContext.Session.Remove("SavedTitle");
                    HttpContext.Session.Remove("SavedDescription");
                }
                HttpContext.Session.Remove("CreateSessionMessage");
                HttpContext.Session.Remove("IsMessagePositive");
            }
            _logger.LogInformation("Linchevsky id: " + id);
            return View();
        }
        [HttpPost]
        public RedirectToActionResult CreatePost(ForumPostFormModel formModel)
        {
            Guid userId;
            if (!_validationService.Validate(formModel.Content, ValidationTerms.NotEmpty))
            {
                CheckInvalidForm("Content can't be empty", formModel.Content, formModel.ReplyId);
            }
            else
            {
                try
                {
                    userId = Guid.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Sid)?.Value);
                    _dataContext.Posts.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Content = formModel.Content,
                        ReplyId = String.IsNullOrEmpty(formModel.ReplyId)
                        ? null
                        : Guid.Parse(formModel.ReplyId),
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        TopicId = Guid.Parse(formModel.TopicId)
                    });
                    _dataContext.SaveChanges();
                    HttpContext.Session.SetInt32("IsMessagePositive", 1);
                    HttpContext.Session.SetString("CreateSectionMessage",
                        "Added successfully");
                }
                catch
                {
                    CheckInvalidForm("Authorization denied", formModel.Content, formModel.ReplyId);
                }
            }
            return RedirectToAction(nameof(Topics), new { id = formModel.TopicId });
        }
        public IActionResult Themes([FromRoute] String id)
        {
            Guid themeId;
            try
            {
                themeId = Guid.Parse(id);
            }
            catch
            {
                themeId = Guid.Empty;
            }
            var theme = _dataContext.Themes.Find(themeId);
            ForumThemesModel model = new()
            {
                UserCanCreate = HttpContext.User.Identity?.IsAuthenticated == true,
                Title = theme.Title,
                ThemeId = id,
                Topics = _dataContext
                .Topics
                .Where(t => t.Deleted == null && t.ThemeId == themeId)
                .Select(t => new ForumTopicViewModel()
                {
                    Title = t.Title,
                    Description = t.Description + (t.Description.Length > 100 ? "..." : ""),
                    CreatedDtString = DateTime.Today == t.CreatedDt.Date
                    ? "Сьогодні " + t.CreatedDt.ToString("HH:mm")
                    : t.CreatedDt.ToString("dd.MM.yyy HH:mm"),
                    UrlIdString = t.Id.ToString()
                })
                .ToList()
            };
            return View(model);
        }
        [HttpPost]
        public RedirectToActionResult CreateTopic(ForumTopicFormModel formModel)
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
                    _dataContext.Topics.Add(new()
                    {
                        Id = Guid.NewGuid(),
                        Title = formModel.Title,
                        Description = formModel.Description,
                        CreatedDt = DateTime.Now,
                        AuthorId = userId,
                        ThemeId = Guid.Parse(formModel.ThemeId)
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

            return RedirectToAction(nameof(Themes),
                new { id = formModel.ThemeId });
        }
    }
}