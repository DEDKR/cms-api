
using CmsApi.Common;
using CmsApi.DB;
using CmsApi.Extensions;
using CmsApi.Http.Handlers.Implementations;
using CmsApi.Http.Handlers.Interfaces;
using CmsApi.Repositories.Implementations;
using CmsApi.Repositories.Interfaces;
using CmsApi.Services.Implementations;
using CmsApi.Services.Interfaces;
using CmsApi.Services.Interfaces.CmsApi.Services.Interfaces;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace CmsApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
            builder.Services.AddScoped<ICaseRepository, CaseRepository>();
            builder.Services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();
            builder.Services.AddScoped<IECmsTokenRepository, ECmsTokenRepository>();
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IECmsAuthService, ECmsAuthService>();
            builder.Services.AddScoped<ICmsLoginService, CmsLoginService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddHttpClient<ICmsLoginHttpHandler, CmsLoginHttpHandler>();
            builder.Services.AddHttpClient<ICmsHttpHandler, CmsHttpHandler>();
            builder.Services.AddScoped<IMeetingRepository, MeetingRepository>();
            builder.Services.AddScoped<IDocumentService, DocumentService>();
            builder.Services.AddScoped<ICaseService, CaseService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped<IPasswordService, PasswordService>();
            builder.Services.AddScoped<ITokenService, TokenService>();

            builder.Services.AddAuthenticationServices(builder.Configuration);



            builder.Services.Configure<CmsApiSettings>(
                                         builder.Configuration.GetSection("CmsApiSettings"));

            builder.Services.Configure<VerificatedAsanCertificates>(
                                         builder.Configuration.GetSection("VerificatedAsanCertificates"));


            builder.Services.AddCorsServices(builder.Configuration);
            builder.Services.AddSwaggerServices();

            // Add services to the container.
            builder.Services.AddScoped<IDbConnection>(sp =>
                 new SqlConnection(builder.Configuration.GetConnectionString("MsSqlDb")));
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseCors(CorsExtensions.CorsPolicyName);
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
