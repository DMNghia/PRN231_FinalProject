using FinalProject.Models;
using FinalProject.Security;
using FinalProject.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<JwtFilter>();
});

// Add jwt config
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
    .AddCookie(options =>
    {
        options.Cookie.IsEssential = true;
    })
    .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
    {
        options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
        options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
        options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");
    });

// Add email settings
var mailsettings = builder.Configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailsettings);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
});

// Add cors config
builder.Services.AddCors();

// Add Razor page config
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Define Singleton
builder.Services.AddSingleton<Prn231_FinalProjectContext>();
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddSingleton<EmailService>();
builder.Services.AddSingleton<AuthService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCookiePolicy();

app.UseSession();

// Use cors
app.UseCors(options => options.AllowAnyOrigin()
	.AllowAnyHeader()
	.AllowAnyMethod());

//app.UseAuthentication();

//app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapRazorPages();

app.Run();
