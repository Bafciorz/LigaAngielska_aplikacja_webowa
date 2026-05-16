using Microsoft.EntityFrameworkCore;
using Liga.Web.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(10);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    }
);

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite("Data Source=liga.db"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();





app.Use(async (ctx, next) =>
{
    await next();

    if(ctx.Response.StatusCode == 404 && !ctx.Response.HasStarted)
    {
        //Re-execute the request so the user gets the error page
        string originalPath = ctx.Request.Path.Value;
        ctx.Items["originalPath"] = originalPath;
        ctx.Request.Path = "/IO/Logowanie";
        await next();
    }
});


app.UseSession();

app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();




using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // Pobieramy nasz kontekst bazy danych z kontenera wstrzykiwania zależności
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Uruchamiamy naszą klasę i przekazujemy jej kontekst
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        // Jeśli coś pójdzie nie tak (np. błąd w SQL), zaloguj błąd w konsoli
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Wystąpił krytyczny błąd podczas seedowania bazy danych.");
    }
}

app.Run();
