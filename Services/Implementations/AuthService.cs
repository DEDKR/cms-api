using CmsApi.DTOs.AuthDtos;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Interfaces;
using CmsApi.Services.Interfaces.CmsApi.Services.Interfaces;

namespace CmsApi.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public AuthService(
            IUserRepository userRepository,
            ITokenRepository tokenRepository,
            IPasswordService passwordService,
            ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        public async Task<LoginResponse> LoginAsync(
            LoginRequest request)
        {
            try
            {


                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    throw new UnauthorizedAccessException(
                        "Username or password is incorrect");
                }

                var user = await _userRepository
                    .GetByUsernameAsync(request.Username);

                // User mövcud deyilsə generic error qaytarırıq.
                // Beləliklə username enumeration qarşısını alırıq.
                if (user == null)
                {
                    throw new UnauthorizedAccessException(
                        "Username or password is incorrect");
                }

                // User deaktivdirsə loginə icazə verilmir.
                if (!user.IsActive)
                {
                    throw new UnauthorizedAccessException(
                        "Username or password is incorrect");
                }

                // Account lockout yoxlanılır.
                if (user.LockoutUntil.HasValue &&
                    user.LockoutUntil.Value > DateTime.Now)
                {
                    throw new UnauthorizedAccessException(
                        "Account is temporarily locked");
                }

                // Password yoxlanılır.
                bool passwordValid;

                if (!string.IsNullOrWhiteSpace(user.PassHash))
                {
                    passwordValid = _passwordService.VerifyPassword(
                        request.Password,
                        user.PassHash);
                }
                else
                {
                    passwordValid = request.Password == user.Password;
                }

                if (!passwordValid)
                {
                    await _userRepository.RegisterFailedLoginAsync(
                        user.UserId,
                        maxAttempts: 5,
                        lockoutMinutes: 15);

                    throw new UnauthorizedAccessException(
                        "Username or password is incorrect");
                }

                // Uğurlu login olduqda failed attempts sıfırlanır.
                await _userRepository.ResetLoginAttemptsAsync(
                    user.UserId);

                // Access token
                var accessToken =
                    _tokenService.GenerateAccessToken(user);

                var accessTokenExpiresAt =
                    _tokenService.GetAccessTokenExpiration();

                // Yeni refresh token
                var refreshToken =
                    _tokenService.GenerateRefreshToken();

                // DB-də yalnız hash saxlanılır.
                var refreshTokenHash =
                    _tokenService.HashRefreshToken(refreshToken);

                // Refresh token expiry
                var refreshTokenExpiresAt =
                    DateTime.UtcNow.AddDays(7);

                await _tokenRepository.UpsertRefreshTokenAsync(
                    user.UserId,
                    refreshTokenHash,
                    refreshTokenExpiresAt);

                return new LoginResponse
                {
                    Token = accessToken,

                    RefreshToken = refreshToken,

                    ExpiresAt = accessTokenExpiresAt,

                    User = new UserDto
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        RoleName = user.Role,
                        FullName = user.FirstName + " " + user.LastName + " " + user.FatherName,
                        IsPassChangeRequired = user.IsPassChangeRequired
                    }
                };
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<RefreshTokenResponse> RefreshAsync(

    RefreshTokenRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token");
            }

            // Plain refresh tokeni hash edirik.
            var tokenHash =
                _tokenService.HashRefreshToken(
                    request.RefreshToken);

            // Hash ilə DB-dən refresh tokeni tapırıq.
            var storedToken =
                await _tokenRepository.GetRefreshTokenAsync(
                    tokenHash);

            if (storedToken == null)
            {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token");
            }

            // Token revoke olunubsa istifadə edilə bilməz.
            if (storedToken.IsRevoked)
            {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token");
            }

            // Tokenin vaxtı keçibsə istifadə edilə bilməz.
            if (storedToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException(
                    "Refresh token expired");
            }

            // User-i DB-dən yenidən götürürük.
            var user =
                await _userRepository.GetByIdAsync(
                    storedToken.UserId);

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token");
            }

            // Yeni access token yaradırıq.
            var accessToken =
                _tokenService.GenerateAccessToken(user);

            // Access token expiry.
            var accessTokenExpiresAt =
                _tokenService.GetAccessTokenExpiration();

            // Yeni refresh token yaradırıq.
            var newRefreshToken =
                _tokenService.GenerateRefreshToken();

            // DB-də yalnız hash saxlanılır.
            var newRefreshTokenHash =
                _tokenService.HashRefreshToken(
                    newRefreshToken);

            // Config-dən refresh token expiry götürülür.
            var refreshTokenExpiresAt =
                _tokenService.GetRefreshTokenExpiration();

            // Mövcud user-in refresh tokenini yeniləyirik.
            await _tokenRepository.UpsertRefreshTokenAsync(
                user.UserId,
                newRefreshTokenHash,
                refreshTokenExpiresAt);

            return new RefreshTokenResponse
            {
                Token = accessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = accessTokenExpiresAt
            };
        }

        public async Task LogoutAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException(
                    "Invalid user");
            }

            await _tokenRepository.RevokeRefreshTokenAsync(userId);
        }

        public async Task ChangePasswordAsync(
     int userId,
     ChangePasswordRequest request)
        {
            if (userId <= 0)
            {
                throw new UnauthorizedAccessException(
                    "Invalid user");
            }

            if (request == null ||
                string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
            {
                throw new ArgumentException(
                    "Password information is required");
            }

            if (request.NewPassword.Length < 8)
            {
                throw new ArgumentException(
                    "Password must contain at least 8 characters");
            }

            // User-i tapırıq.
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Invalid user");
            }

            bool passwordValid;

            if (!string.IsNullOrWhiteSpace(user.PassHash))
            {
                passwordValid = _passwordService.VerifyPassword(
                    request.CurrentPassword,
                    user.PassHash);
            }
            else
            {
                passwordValid = request.CurrentPassword == user.Password;
            }

            if (!passwordValid)
            {
                await _userRepository.RegisterFailedLoginAsync(
                    user.UserId,
                    maxAttempts: 5,
                    lockoutMinutes: 15);

                throw new UnauthorizedAccessException(
                     "Current password is incorrect");
            }

           
            //// Yeni password köhnə password ilə eyni olmasın.
            //if (_passwordService.VerifyPassword(
            //    request.NewPassword,
            //    user.PassHash))
            //{
            //    throw new ArgumentException(
            //        "New password must be different from current password");
            //}

            // Yeni password hash edilir.
            var newPassHash =
                _passwordService.HashPassword(
                    request.NewPassword);

            // Password update edilir və
            // məcburi dəyişiklik flag-i söndürülür.
            var updated =
                await _userRepository.UpdatePasswordAsync(
                    userId,
                    newPassHash,
                    request.NewPassword,
                    false);

            if (!updated)
            {
                throw new Exception(
                    "Password could not be updated");
            }

            // Password dəyişdikdən sonra mövcud refresh tokeni
            // revoke edirik.
            await _tokenRepository.RevokeRefreshTokenAsync(
                userId);
        }
    }
}