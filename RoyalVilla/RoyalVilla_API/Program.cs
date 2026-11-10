using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Models.DTO;
using RoyalVilla_API.Services;
using Scalar.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JWTSettings")["Secret"]);

builder.Services.AddAuthentication(opt =>
{
	opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
	opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
	opt.RequireHttpsMetadata = false;
	opt.SaveToken = true;
	opt.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuerSigningKey = true,
		IssuerSigningKey = new SymmetricSecurityKey(key),
		ValidateIssuer = false,
		ValidateAudience = false,
		ValidateLifetime = true,
		ClockSkew = TimeSpan.Zero
	};
});

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
	opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(opt =>
{
	opt.AddDocumentTransformer((document, context, cancellationToken) =>
	{
		document.Components ??= new();
		document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>
		{
			["Bearer"] = new OpenApiSecurityScheme
			{
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				Description = "Enter JWT bearer token"
			}
		};

		document.Security =
		[
			new OpenApiSecurityRequirement
			{
				{ new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }
			}
		];

		return Task.CompletedTask;
	});
});

builder.Services.AddAutoMapper(opt =>
{
	opt.CreateMap<Villa, VillaCreateDTO>().ReverseMap();
	opt.CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
	opt.CreateMap<Villa, VillaDTO>().ReverseMap();
	opt.CreateMap<VillaUpdateDTO, VillaDTO>().ReverseMap();
	opt.CreateMap<User, UserDTO>().ReverseMap();
});
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();
await SeedData(app);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();


static async Task SeedData(WebApplication app)
{
	using var scope = app.Services.CreateAsyncScope();
	var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

	await context.Database.MigrateAsync();
}