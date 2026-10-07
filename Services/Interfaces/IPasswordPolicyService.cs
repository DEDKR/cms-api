namespace CmsApi.Services.Interfaces
{
    public interface IPasswordPolicyService
    {
        /// <summary>
        /// Password Content(1) layihəsindəki qaydalara uyğundursa null,
        /// uyğun deyilsə istifadəçiyə göstəriləcək Azərbaycan dilində xəta qaytarır.
        /// </summary>
        string? Validate(string? password, bool passwordRequired = true);

        int MinLength { get; }
    }
}
