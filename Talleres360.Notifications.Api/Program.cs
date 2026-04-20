using Resend;
using Scalar.AspNetCore;
using Talleres360.Notifications.Api.Interfaces;
using Talleres360.Notifications.Api.Middleware;
using Talleres360.Notifications.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["ResendSettings:ApiKey"] ?? "";
});
builder.Services.AddTransient<IResend, ResendClient>();

builder.Services.AddScoped<IEmailService, ResendEmailService>();
builder.Services.AddScoped<ITemplateService, TemplateService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.AddApiKeyAuthentication("ApiKey", scheme =>
        {
            scheme.Name = "X-Api-Key";
            scheme.Value = app.Configuration["NotificationsApi:ApiKey"] ?? "";
        });
    });
}

app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();

app.Run();
