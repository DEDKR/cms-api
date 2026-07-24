namespace CmsApi.Extensions
{
    public static class CorsExtensions
    {
        public const string CorsPolicyName = "DefaultCorsPolicy";

        public static IServiceCollection AddCorsServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var origins = configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>();

            services.AddCors(options =>
            {
                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy
                        .WithOrigins(origins!)
                        .AllowAnyHeader()
                        //.AllowCredentials()
                        .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
