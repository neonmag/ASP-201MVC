namespace ASP_201MVC.Services.Validation
{
    public interface IValidationService
    {
        bool Validate(String source, params ValidationTerms[] terms);
    }
}
