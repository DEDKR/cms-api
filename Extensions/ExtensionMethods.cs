using System.Data.Common;

namespace CmsApi.ExtensionMethods
{
    public static class ExtensionMethods
    {
        public static T SafeGet<T>(this DbDataReader reader, string columnName)
        {
            var ordinal = reader.GetOrdinal(columnName);

            if (reader.IsDBNull(ordinal))
            {
                // Nullable tipdirsə null qaytarır, yoxsa default(T)
                if (Nullable.GetUnderlyingType(typeof(T)) != null || !typeof(T).IsValueType)
                    return default!;
                return (T)Activator.CreateInstance(typeof(T))!;
            }

            var value = reader.GetValue(ordinal);

            // Tipə uyğun convert
            if (typeof(T) == typeof(string))
                return (T)(object)value.ToString()!;

            if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
                return (T)(object)Convert.ToInt32(value);

            if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
                return (T)(object)Convert.ToInt64(value);

            if (typeof(T) == typeof(decimal) || typeof(T) == typeof(decimal?))
                return (T)(object)Convert.ToDecimal(value);

            if (typeof(T) == typeof(double) || typeof(T) == typeof(double?))
                return (T)(object)Convert.ToDouble(value);

            if (typeof(T) == typeof(float) || typeof(T) == typeof(float?))
                return (T)(object)Convert.ToSingle(value);

            if (typeof(T) == typeof(bool) || typeof(T) == typeof(bool?))
                return (T)(object)Convert.ToBoolean(value);

            if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
            {
                if (value is DateTime dt)
                    return (T)(object)dt;

                var str = value.ToString();
                if (DateTime.TryParse(str, out var parsedDt))
                    return (T)(object)parsedDt;

                if (int.TryParse(str, out var year))
                    return (T)(object)new DateTime(year, 1, 1);

                return default(T)!;
            }

            // List<string> üçün JSON deserialize
            if (typeof(T) == typeof(List<string>))
            {
                var str = value.ToString()!;
                if (string.IsNullOrWhiteSpace(str))
                    return (T)(object)new List<string>();

                // Əgər DB-də "1115001158,1125140603" kimi saxlanıbsa
                var list = str.Split(',', StringSplitOptions.RemoveEmptyEntries)
                              .Select(x => x.Trim())
                              .ToList();
                return (T)(object)list;
            }

            // fallback
            return (T)value!;
        }

    }
}
