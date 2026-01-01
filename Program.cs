using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Umbraco.Extensions;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ✅ API Controller aktivieren
builder.Services.AddControllers();

// ✅ CORS (Lokal + GitHub Pages)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "http://127.0.0.1:5173",
                "http://localhost:5173",
                "https://noor8686.github.io"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

// ✅ Umbraco: Stelle sicher, dass AddComposers korrekt verwendet wird
builder.CreateUmbracoBuilder()
    .AddBackOffice()  // Umbraco Backoffice für Admin-Oberfläche
    .AddWebsite()     // Umbraco Website-Konfiguration
    .Build();         // Baut die Umbraco-Konfiguration

var app = builder.Build();

// ✅ Asynchrone Umbraco-Initialisierung
await app.BootUmbracoAsync();

// ✅ CORS-Konfiguration vor anderen Middleware
app.UseCors("Frontend");

// ✅ HTTPS-Weiterleitung aktivieren (wenn notwendig)
app.UseHttpsRedirection();

// ✅ Statische Dateien aktivieren (falls benötigt)
app.UseStaticFiles();

// ✅ API Routing aktivieren
app.UseRouting();

// ✅ API-Controller-Routen aktivieren
app.MapControllers();

// ✅ Umbraco Middleware und Endpunkte konfigurieren
app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();  // Backoffice Middleware (Admin-Oberfläche)
        u.UseWebsite();      // Website Middleware (Frontend)
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();  // Backoffice Endpunkte
        u.UseWebsiteEndpoints();     // Website Endpunkte
    });

// ✅ Anwendung starten
await app.RunAsync();
