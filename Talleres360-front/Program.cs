using Talleres360_front.Middlewares;
using Talleres360_front.Services;
using Talleres360_front.Binders;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

builder.Services.AddHttpClient("API", client =>
{
    string baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
        ?? throw new InvalidOperationException("ApiSettings:BaseUrl no está configurado.");
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<TallerService>();
builder.Services.AddScoped<ClienteService>();
builder.Services.AddScoped<VehiculoService>();
builder.Services.AddScoped<CitaService>();
builder.Services.AddScoped<TrabajoService>();
builder.Services.AddScoped<InventarioService>();
builder.Services.AddScoped<ServicioService>();
builder.Services.AddScoped<PresupuestoService>();
builder.Services.AddScoped<FacturaService>();

builder.Services.AddAuthentication()
.AddCookie("ExternalCookie", o => o.ExpireTimeSpan = TimeSpan.FromMinutes(5))
.AddGoogle(o =>
{
    o.SignInScheme  = "ExternalCookie";
    o.ClientId      = builder.Configuration["Authentication:Google:ClientId"]!;
    o.ClientSecret  = builder.Configuration["Authentication:Google:ClientSecret"]!;
    o.CallbackPath  = "/auth/google-callback";
    o.SaveTokens    = false;
});

WebApplication app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseMiddleware<RefreshTokenMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
