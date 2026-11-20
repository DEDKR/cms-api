using System.Text.RegularExpressions;
using CmsApi.Services.Interfaces;

namespace CmsApi.Services.Implementations
{
    /// <summary>
    /// Content(1) -> ValidatePasswordPolicy qaydalarının eyni variantı.
    /// </summary>
    public class PasswordPolicyService : IPasswordPolicyService
    {
        private readonly IConfiguration _configuration;

        public PasswordPolicyService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int MinLength
        {
            get
            {
                var configured =
                    _configuration.GetValue<int?>("PasswordMinLength");

                return configured.HasValue && configured.Value >= 8
                    ? configured.Value
                    : 8;
            }
        }

        public string? Validate(
            string? password,
            bool passwordRequired = true)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return passwordRequired
                    ? "Şifrə tələb olunur."
                    : null;
            }

            if (password.Length < MinLength)
            {
                return $"Şifrə ən azı {MinLength} simvol olmalıdır.";
            }

            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                return "Şifrədə ən azı bir böyük hərf olmalıdır.";
            }

            if (!Regex.IsMatch(password, "[a-z]"))
            {
                return "Şifrədə ən azı bir kiçik hərf olmalıdır.";
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                return "Şifrədə ən azı bir rəqəm olmalıdır.";
            }

            if (!Regex.IsMatch(password, "[^A-Za-z0-9]"))
            {
                return "Şifrədə ən azı bir xüsusi simvol olmalıdır.";
            }

            return null;
        }
    }
}
