using Microsoft.Identity.Client;

namespace ASP_201MVC.Models.User
{
    public class ProfileModel
    {
        public Guid Sid { get; set; }
        public string RealName { get; set; }
        public bool IsRealNamePublic { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public bool IsEmailPublic { get; set; }
        public string Avatar { get; set; }
        public DateTime RegisterDt { get; set; }
        public DateTime? LastEnterDt { get; set; }
        public bool IsDatetimesPublic { get; set; }
        public bool isEmailPublic { get; set; }
        public bool IsEmailConfirmed { get; set; }
        // Является ли данный провиль персональным (для аутентифицированого пользователя)
        public bool IsPersonal { get; set; }

        public ProfileModel(Data.Entity.User user)
        {
            /*
            Sid = user.Id;
            RealName = user.RealName;
            IsRealNamePublic = user.IsRealNamePublic;
            Login = user.Login;
            Email = user.Email;
            IsEmailPublic = user.IsEmailPublic;
            Avatar = user.Avatar;
            RegisterDt = user.RegisterDt;
            LastEnterDt = user.LastEnterDt;
            IsDatetimesPublic = user.IsDtPublic;
            */
            var thisProps = this.GetType().GetProperties();
            foreach (var prop in user.GetType().GetProperties())
            {
                var thisProp = thisProps.FirstOrDefault(p => p.Name == prop.Name && p.PropertyType.IsAssignableFrom(prop.PropertyType));

                thisProp?.SetValue(this, prop.GetValue(user));
            }
            this.IsEmailConfirmed = user.EmailCode is null;
        }
    }
}
