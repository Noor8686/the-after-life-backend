using Umbraco.Cms.Web.Common.ApplicationBuilder;

var builder = WebApplication.CreateBuilder(args);

// ================================
// 1️⃣ Services
// ================================

// API Controller
builder.Services.AddControllers();

// CORS (lokal + GitHub Pages)
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

// ================================
// 2️⃣ Umbraco
// ================================
builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .Build();

var app = builder.Build();

// Umbraco booten
await app.BootUmbracoAsync();

// ================================
// 3️⃣ Middleware
// ================================
app.UseHttpsRedirection();
app.UseStaticFiles();

// CORS MUSS vor MapControllers
app.UseCors("Frontend");

// ================================
// 4️⃣ API Routen
// ================================
app.MapControllers();

// ================================
// 5️⃣ Umbraco Pipeline
// ================================
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
