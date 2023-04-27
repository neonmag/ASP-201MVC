using System.Net.Mail;
using System.Runtime.Intrinsics.X86;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.Net;

namespace ASP_201MVC.Services.Email
{
    public class GmailService: IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GmailService> _logger;

        public GmailService(IConfiguration configuration, ILogger<GmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public bool Send(string mailTemplate, object model)
        {
            String? template = null;
            String[] filenames = new String[]
            {
                mailTemplate,
                mailTemplate + ".html",
                "Services/Email/" + mailTemplate,
                "Services/Email/" + mailTemplate + ".html"
            };
            foreach (String filename in filenames)
            {
                if (System.IO.File.Exists(filename))
                {
                    template = System.IO.File.ReadAllText(filename);
                    break;
                }
            }
            if (template is null)
            {
                throw new ArgumentException($"Template '{mailTemplate}' not found");
            }
            String? host = _configuration["Smtp:Gmail:Host"];
            if (host is null) throw new MissingFieldException($"Missing configuration 'Smtp:Gmail:Host'");
            String? mailbox = _configuration["Smtp:Gmail:Email"];
            if (host is null) throw new MissingFieldException($"Missing configuration 'Smtp:Gmail:Email'");
            String? password = _configuration["Smtp:Gmail:Password"];
            if (host is null) throw new MissingFieldException($"Missing configuration 'Smtp:Gmail:Password'");

            int port;
            try { port = Convert.ToInt32(_configuration["Smtp:Gmail:Port"]); }
            catch { throw new MissingFieldException($"Missing configuration 'Smtp:Gmail:Port'"); }
            bool ssl;
            try { ssl = Convert.ToBoolean(_configuration["Smtp:Gmail:Ssl"]); }
            catch { throw new MissingFieldException($"Missing configuration 'Smtp:Gmail:Ssl'"); }

            String? userEmail = null;
            foreach (var prop in model.GetType().GetProperties())
            {
                if (prop.Name == "Email") userEmail = prop.GetValue(model)?.ToString();
                String placeholder = $"{{{prop.Name}}}";
                if (template.Contains(placeholder))
                {
                    template = template.Replace(placeholder, prop.GetValue(model)?.ToString() ?? "");
                }
            }
            if (userEmail is null)
            {
                throw new ArgumentException("No 'Email' property in model");
            }
            using SmtpClient smtpClient = new(host, port)
            {
                EnableSsl = ssl,
                Credentials = new NetworkCredential(mailbox, password)
            };
            MailMessage mailMessage = new()
            {
                From = new MailAddress(mailbox),
                Subject = "ASP Project",
                Body = template,
                IsBodyHtml = true
            };
            mailMessage.To.Add(userEmail);
            try
            {
                smtpClient.Send(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Send Email exception {ex}", ex.Message);
                return false;
            }
        }
    }
}
