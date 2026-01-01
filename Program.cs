using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;

namespace TheAfterLifeCMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Services
            builder.Services.AddControllers();

            // Authorization registrieren (sonst UseAuthorization entfernen)
            builder.Services.AddAuthorization();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                    policy
                        .WithOrigins("http://localhost:5500", "http://127.0.0.1:5500")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            builder.CreateUmbracoBuilder()
                   .AddBackOffice()
                   .AddWebsite()
                   .Build();

            var app = builder.Build();

            await app.BootUmbracoAsync();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseStaticFiles();

            app.UseRouting();
            app.UseCors("Frontend");
            app.UseAuthorization();
            app.MapControllers();

            app.UseUmbraco()
               .WithMiddleware(u =>
               {
                   u.UseBackOffice();
                   u.UseWebsite();
               })
               .WithEndpoints(u =>
               {
                   u.UseBackOfficeEndpoints();
                   u.UseWebsiteEndpoints();
               });

            await app.RunAsync();
        }
    }
}
