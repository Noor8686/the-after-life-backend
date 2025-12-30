using Umbraco.Cms.Web.Common.ApplicationBuilder;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// ================================
// 1️⃣ Services (GANZ OBEN)
// ================================

// ✅ API Controller aktivieren
builder.Services.AddControllers();

// ✅ CORS für deine Website (The After Life)
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "http://127.0.0.1:5173",
                "http://localhost:5173"
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
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();

// ================================
// 3️⃣ Middleware (Reihenfolge wichtig!)
// ================================
app.UseHttpsRedirection();
app.UseStaticFiles();

// ✅ CORS MUSS vor MapControllers kommen
app.UseCors("Frontend");

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

// ================================
// 4️⃣ API Routen aktivieren
// ================================
app.MapControllers();

await app.RunAsync();
