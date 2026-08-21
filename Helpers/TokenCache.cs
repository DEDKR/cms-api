using CmsApi.Entities;
using CmsApi.Entities.CmsApi.Entities;

namespace CmsApi.Helpers
{
    public static class TokenCache
    {
        private static readonly object _lock = new();

        private static Token? _token;

        public static Token? Get()
        {
            lock (_lock)
            {
                return _token;
            }
        }

        public static void Set(Token token)
        {
            lock (_lock)
            {
                _token = token;
            }
        }

        public static void Clear()
        {
            lock (_lock)
            {
                _token = null;
            }
        }
    }
}
