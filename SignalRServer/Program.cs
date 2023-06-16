using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SignalRServer.Authorization;
using SignalRServer.Hubs;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAny", policy => policy.AllowAnyOrigin()
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod());
});

JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "oidc";
})
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:5263";
        options.ClientId = "webAppClient";
        options.ClientSecret = "webAppClientSecret";
        options.ResponseType = "code";
        options.CallbackPath = "/signin-oidc";
        options.SaveTokens = true;
        options.RequireHttpsMetadata = false;
    })
    .AddJwtBearer(options =>
    {
        options.Authority = "https://localhost:5263";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
        options.RequireHttpsMetadata = false;
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (path.StartsWithSegments("/messageHub"))
                {

                    if (string.IsNullOrWhiteSpace(accessToken))
                    {
                        accessToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                    }
                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        context.Token = accessToken;
                    }
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicAuth", policy => policy.RequireAuthenticatedUser());
    options.AddPolicy("IsAdmin", policy => policy.Requirements.Add(new RoleRequirement("admin")));
});


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAny");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<MessageHub>("/messageHub");
app.UseBlazorFrameworkFiles();
app.Run();
