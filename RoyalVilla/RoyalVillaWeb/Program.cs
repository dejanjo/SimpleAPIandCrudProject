using Microsoft.AspNetCore.Authentication.Cookies;
using RoyalVilla.DTO;
using RoyalVillaWeb.Services;
using RoyalVillaWeb.Services.IServices;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
	opt.IdleTimeout = TimeSpan.FromMinutes(60);
	opt.Cookie.HttpOnly = true;
	opt.Cookie.IsEssential = true;
});
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(opt =>
{
	opt.CreateMap<VillaDTO, VillaCreateDTO>().ReverseMap();
	opt.CreateMap<VillaUpdateDTO, VillaDTO>().ReverseMap();
});

builder.Services.AddHttpClient("RoyalVillaAPI", client =>
{
	var villaAPIUrl = builder.Configuration.GetValue<string>("ServiceUrls:VillaAPI");
	client.BaseAddress = new Uri(villaAPIUrl);
	client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(opt =>
	{
		opt.Cookie.HttpOnly = true;
		opt.ExpireTimeSpan = TimeSpan.FromMinutes(60);
		opt.SlidingExpiration = true;
		opt.LoginPath = "/auth/login";
		opt.AccessDeniedPath = "/auth/accessdenied";
	});

builder.Services.AddScoped<IVillaService, VillaService>();
builder.Services.AddScoped<IAuthService, AuthService>();
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
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();


app.Run();
