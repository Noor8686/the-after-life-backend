using Umbraco.Cms.Web.Common.ApplicationBuilder;

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

// ✅ Umbraco
builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .Build();

var app = builder.Build();

await app.BootUmbracoAsync();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ CORS muss vor MapControllers
app.UseCors("Frontend");

// ✅ API Routen aktivieren
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
