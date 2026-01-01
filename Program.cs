using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using Microsoft.AspNetCore.Builder;

namespace TheAfterLifeCMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // API Controller aktivieren
            builder.Services.AddControllers();

            // CORS (Lokal + GitHub Pages)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Frontend", policy =>
                    policy
                        .WithOrigins("http://localhost:5500", "http://127.0.0.1:5500")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());
            });

            // Umbraco spezifische Konfigurationen
            builder.CreateUmbracoBuilder()
                .AddBackOffice()
                .AddWebsite()
                .Build();

            var app = builder.Build();

            // Asynchrone Umbraco-Initialisierung
            await app.BootUmbracoAsync();

            // HTTPS-Weiterleitung (nur außerhalb Development)
            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }

            // Statische Dateien
            app.UseStaticFiles();

            // Routing -> CORS -> Controller-Mapping
            app.UseRouting();
            app.UseCors("Frontend");

            // Falls später Authorization benötigt wird
            app.UseAuthorization();

            // API-Controller-Routen aktivieren
            app.MapControllers();

            // Umbraco Middleware und Endpunkte konfigurieren
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

            // Anwendung starten (asynchron)
            await app.RunAsync();
        }
    }
}
