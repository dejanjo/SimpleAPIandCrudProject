using Microsoft.EntityFrameworkCore;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
	opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddAutoMapper(opt =>
{
	opt.CreateMap<Villa, VillaCreateDTO>().ReverseMap();
	opt.CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
	opt.CreateMap<Villa, VillaDTO>().ReverseMap();
	opt.CreateMap<VillaUpdateDTO, VillaDTO>().ReverseMap();
});
var app = builder.Build();

await SeedData(app);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


static async Task SeedData(WebApplication app)
{
	using var scope = app.Services.CreateAsyncScope();
	var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

	await context.Database.MigrateAsync();
}