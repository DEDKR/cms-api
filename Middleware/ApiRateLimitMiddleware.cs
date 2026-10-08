using System.Collections.Concurrent;
using System.Security.Claims;
using System.Text.Json;

namespace CmsApi.Middleware
{
    /// <summary>
    /// Content(1) layihəsindəki ApiRateLimitHandler məntiqinin ASP.NET Core variantı.
    ///
    /// - Hər endpoint üçün ayrıca sayğac: userId|ip|/api/example
    /// - 60 saniyədə 20 sorğu (appsettings-dən dəyişilə bilər)
    /// - Limit bir endpoint-də aşılarsa həmin user/IP üçün BÜTÜN /api/* bloklanır
    /// - 1-ci pozuntu: 15 dəqiqə
    /// - 2-ci pozuntu: 60 dəqiqə
    /// - 3-cü və sonrakılar: 12 saat
    /// - Pozuntu mərhələsi 24 saatdan sonra sıfırlanır
    /// </summary>
    public sealed class ApiRateLimitMiddleware
    {
        private const int DefaultWindowSeconds = 60;
        private const int DefaultMaxRequests = 20;

        private const int FirstBlockMinutes = 1;
        private const int SecondBlockMinutes = 2;
        private const int ThirdBlockMinutes = 3; // 12 saat

        private const int ViolationPeriodHours = 24;

        private static readonly ConcurrentDictionary<string, EndpointRateLimitState>
            EndpointAttempts = new();

        private static readonly ConcurrentDictionary<string, ClientBlockState>
            ClientBlocks = new();

        private static DateTime _lastCleanupUtc = DateTime.UtcNow;
        private static readonly object CleanupLock = new();

        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;

        public ApiRateLimitMiddleware(
            RequestDelegate next,
            IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (ShouldSkip(context))
            {
                await _next(context);
                return;
            }

            var retryAfterSeconds = CheckRequest(context);

            if (retryAfterSeconds.HasValue)
            {
                await WriteBlockedResponseAsync(
                    context,
                    retryAfterSeconds.Value);
                return;
            }

            await _next(context);
        }

        private int? CheckRequest(HttpContext context)
        {
            var now = DateTime.UtcNow;
            CleanupExpiredEntries(now);

            var clientKey = BuildClientKey(context);

            var clientState = ClientBlocks.GetOrAdd(
                clientKey,
                _ => new ClientBlockState());

            // Əvvəl ümumi blok yoxlanılır.
            // Bir endpoint limiti aşdıqda bütün /api/* burada bloklanır.
            lock (clientState)
            {
                if (clientState.BlockedUntilUtc.HasValue &&
                    clientState.BlockedUntilUtc.Value > now)
                {
                    return RemainingSeconds(
                        clientState.BlockedUntilUtc.Value,
                        now);
                }

                if (clientState.BlockedUntilUtc.HasValue &&
                    clientState.BlockedUntilUtc.Value <= now)
                {
                    clientState.BlockedUntilUtc = null;
                }

                if (clientState.ViolationPeriodStartedAt.HasValue &&
                    (now - clientState.ViolationPeriodStartedAt.Value)
                        .TotalHours >= ViolationPeriodHours)
                {
                    clientState.ViolationPeriodStartedAt = null;
                    clientState.ViolationCount = 0;
                    clientState.LastViolationUtc = null;
                }
            }

            // Hər endpoint üçün ayrıca sayğac.
            var endpointKey = BuildEndpointKey(context, clientKey);

            var endpointState = EndpointAttempts.GetOrAdd(
                endpointKey,
                _ => new EndpointRateLimitState
                {
                    WindowStartedAt = now,
                    RequestCount = 0,
                    LastRequestUtc = now
                });

            var limitExceeded = false;

            lock (endpointState)
            {
                var windowSeconds = GetIntSetting(
                    "ApiRateLimitWindowSeconds",
                    DefaultWindowSeconds);

                var maxRequests = GetIntSetting(
                    "ApiRateLimitMaxRequests",
                    DefaultMaxRequests);

                if ((now - endpointState.WindowStartedAt)
                    .TotalSeconds >= windowSeconds)
                {
                    endpointState.WindowStartedAt = now;
                    endpointState.RequestCount = 0;
                }

                endpointState.RequestCount++;
                endpointState.LastRequestUtc = now;

                // Content(1)-də olduğu kimi limit 20-dirsə 21-ci sorğu blok yaradır.
                if (endpointState.RequestCount > maxRequests)
                {
                    limitExceeded = true;

                    // Pozuntudan sonra həmin endpoint üçün yeni window başlayır.
                    endpointState.WindowStartedAt = now;
                    endpointState.RequestCount = 0;
                }
            }

            if (!limitExceeded)
                return null;

            // Bir endpoint limiti aşdı -> bütün API üçün ümumi blok.
            lock (clientState)
            {
                // Paralel request artıq blok qoyubsa mövcud bloku qaytarırıq.
                if (clientState.BlockedUntilUtc.HasValue &&
                    clientState.BlockedUntilUtc.Value > now)
                {
                    return RemainingSeconds(
                        clientState.BlockedUntilUtc.Value,
                        now);
                }

                if (clientState.ViolationPeriodStartedAt.HasValue &&
                    (now - clientState.ViolationPeriodStartedAt.Value)
                        .TotalHours >= ViolationPeriodHours)
                {
                    clientState.ViolationPeriodStartedAt = null;
                    clientState.ViolationCount = 0;
                }

                if (!clientState.ViolationPeriodStartedAt.HasValue)
                {
                    clientState.ViolationPeriodStartedAt = now;
                    clientState.ViolationCount = 0;
                }

                clientState.ViolationCount++;

                var blockMinutes = clientState.ViolationCount switch
                {
                    1 => FirstBlockMinutes,
                    2 => SecondBlockMinutes,
                    _ => ThirdBlockMinutes
                };

                clientState.BlockedUntilUtc = now.AddMinutes(blockMinutes);
                clientState.LastViolationUtc = now;

                return RemainingSeconds(
                    clientState.BlockedUntilUtc.Value,
                    now);
            }
        }

        private static bool ShouldSkip(HttpContext context)
        {
            if (HttpMethods.IsOptions(context.Request.Method))
                return true;

            var path = context.Request.Path.Value;

            if (string.IsNullOrWhiteSpace(path))
                return true;

            return !path.StartsWith(
                "/api/",
                StringComparison.OrdinalIgnoreCase);
        }

        // Ümumi client key: userId|ip
        private static string BuildClientKey(HttpContext context)
        {
            var userId = context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?
                .Value;

            if (string.IsNullOrWhiteSpace(userId))
                userId = "anonymous";

            return $"{userId}|{ClientIp(context)}";
        }

        // Endpoint key: userId|ip|/api/example
        private static string BuildEndpointKey(
            HttpContext context,
            string clientKey)
        {
            var endpointPath =
                (context.Request.Path.Value ?? "/")
                .TrimEnd('/')
                .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(endpointPath))
                endpointPath = "/";

            return $"{clientKey}|{endpointPath}";
        }

        private static string ClientIp(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(
                    "X-Forwarded-For",
                    out var forwardedValues))
            {
                var forwardedIp = forwardedValues.FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(forwardedIp))
                {
                    return forwardedIp
                        .Split(',')[0]
                        .Trim();
                }
            }

            return context.Connection.RemoteIpAddress?.ToString()
                   ?? "unknown";
        }

        private async Task WriteBlockedResponseAsync(
            HttpContext context,
            int retryAfterSeconds)
        {
            var remainingMinutes = (int)Math.Ceiling(
                retryAfterSeconds / 60.0);

            var messageText = string.Format(
                "Çoxsaylı sorğu qeydə alınmasına görə {0} dəqiqə sonra yenidən cəhd edin.",
                remainingMinutes);

            context.Response.StatusCode =
                StatusCodes.Status429TooManyRequests;

            context.Response.ContentType =
                "application/json; charset=utf-8";

            context.Response.Headers["Retry-After"] =
                retryAfterSeconds.ToString();

            // Content(1)-dəki response strukturu saxlanılır.
            var response = new
            {
                messages = new object[]
                {
                    new
                    {
                        msg_code = 1,
                        msg_text = messageText,
                        blocked_minutes = remainingMinutes,
                        remaining_seconds = retryAfterSeconds,
                        remaining_minutes = remainingMinutes
                    }
                },
                data = new { }
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }

        private static int RemainingSeconds(
            DateTime blockedUntilUtc,
            DateTime now)
        {
            return Math.Max(
                1,
                (int)Math.Ceiling(
                    (blockedUntilUtc - now).TotalSeconds));
        }

        public static int GetRemainingBlockSeconds(
            int userId,
            string ipAddress)
        {
            if (userId <= 0 ||
                string.IsNullOrWhiteSpace(ipAddress))
            {
                return 0;
            }

            return GetRemainingBlockSecondsByKey(
                $"{userId}|{ipAddress.Trim()}");
        }

        public static int GetAnonymousRemainingBlockSeconds(
            string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return 0;

            return GetRemainingBlockSecondsByKey(
                $"anonymous|{ipAddress.Trim()}");
        }

        public static int GetRemainingBlockSecondsByIp(
            string ipAddress)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                return 0;

            var now = DateTime.UtcNow;
            var keySuffix = $"|{ipAddress.Trim()}";
            var maximumRemainingSeconds = 0;

            foreach (var item in ClientBlocks.ToArray())
            {
                if (!item.Key.EndsWith(
                    keySuffix,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var state = item.Value;

                lock (state)
                {
                    if (!state.BlockedUntilUtc.HasValue)
                        continue;

                    if (state.BlockedUntilUtc.Value <= now)
                    {
                        state.BlockedUntilUtc = null;
                        continue;
                    }

                    var remaining = RemainingSeconds(
                        state.BlockedUntilUtc.Value,
                        now);

                    if (remaining > maximumRemainingSeconds)
                        maximumRemainingSeconds = remaining;
                }
            }

            return maximumRemainingSeconds;
        }

        private static int GetRemainingBlockSecondsByKey(
            string clientKey)
        {
            if (!ClientBlocks.TryGetValue(
                    clientKey,
                    out var clientState))
            {
                return 0;
            }

            var now = DateTime.UtcNow;

            lock (clientState)
            {
                if (!clientState.BlockedUntilUtc.HasValue)
                    return 0;

                if (clientState.BlockedUntilUtc.Value <= now)
                {
                    clientState.BlockedUntilUtc = null;
                    return 0;
                }

                return RemainingSeconds(
                    clientState.BlockedUntilUtc.Value,
                    now);
            }
        }

        private void CleanupExpiredEntries(DateTime now)
        {
            if ((now - _lastCleanupUtc).TotalMinutes < 5)
                return;

            lock (CleanupLock)
            {
                if ((now - _lastCleanupUtc).TotalMinutes < 5)
                    return;

                _lastCleanupUtc = now;
            }

            foreach (var item in EndpointAttempts.ToArray())
            {
                var state = item.Value;

                lock (state)
                {
                    if ((now - state.LastRequestUtc).TotalMinutes > 10)
                    {
                        EndpointAttempts.TryRemove(
                            item.Key,
                            out _);
                    }
                }
            }

            foreach (var item in ClientBlocks.ToArray())
            {
                var state = item.Value;

                lock (state)
                {
                    var blockExpired =
                        !state.BlockedUntilUtc.HasValue ||
                        state.BlockedUntilUtc.Value <= now;

                    var violationPeriodExpired =
                        !state.ViolationPeriodStartedAt.HasValue ||
                        (now - state.ViolationPeriodStartedAt.Value)
                            .TotalHours >= ViolationPeriodHours;

                    if (blockExpired && violationPeriodExpired)
                    {
                        ClientBlocks.TryRemove(
                            item.Key,
                            out _);
                    }
                }
            }
        }

        private int GetIntSetting(
            string key,
            int defaultValue)
        {
            var raw = _configuration[key];

            return int.TryParse(raw, out var value) && value > 0
                ? value
                : defaultValue;
        }

        private sealed class EndpointRateLimitState
        {
            public DateTime WindowStartedAt { get; set; }
            public int RequestCount { get; set; }
            public DateTime LastRequestUtc { get; set; }
        }

        private sealed class ClientBlockState
        {
            public DateTime? BlockedUntilUtc { get; set; }
            public DateTime? ViolationPeriodStartedAt { get; set; }
            public int ViolationCount { get; set; }
            public DateTime? LastViolationUtc { get; set; }
        }
    }
}
