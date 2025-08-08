using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);

// 1) Cookies de autenticación
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Autenticacion/Login";
        options.LogoutPath = "/Autenticacion/Logout";
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // solo sobre HTTPS
        options.SlidingExpiration = true;
        // Si estás bajo subruta, NO pongas la subruta aquí; el PathBase se añade solo (ver abajo).
    });

builder.Services.AddControllersWithViews();

// 2) Forwarded headers (muy importante detrás de Nginx/IIS/Azure)
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto |
        ForwardedHeaders.XForwardedHost;
    // Si tu proxy no está en KnownProxies/Networks, limpiamos para aceptar cualquiera (o configúralos explícitos)
    o.KnownNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

// 3) (Opcional) Si la app vive en subruta, toma de env var (o pon literal "/app_siniestros")
var pathBase = app.Configuration["PathBase"]; // por ejemplo, "/app_siniestros"
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
    // Redirige / a /{subruta}/
    app.Use((ctx, next) =>
    {
        if (ctx.Request.Path == "/" && !string.IsNullOrEmpty(ctx.Request.PathBase))
        {
            ctx.Response.Redirect(ctx.Request.PathBase + "/");
            return Task.CompletedTask;
        }
        return next();
    });
}

// Orden correcto de middlewares
app.UseForwardedHeaders();            // primero
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = new FileExtensionContentTypeProvider
    {
        Mappings = { [".webmanifest"] = "application/manifest+json" }
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Redirección raíz a Login (respeta PathBase automáticamente)
app.MapGet("/", ctx =>
{
    ctx.Response.Redirect($"{ctx.Request.PathBase}/Autenticacion/Login");
    return Task.CompletedTask;
});

// Attribute routing si lo usas
app.MapControllers();

// Convencional (fallback)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autenticacion}/{action=Login}/{id?}"
);

app.Run();
