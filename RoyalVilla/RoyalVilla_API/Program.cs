using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla.DTO;
using RoyalVilla_API.Services;
using Scalar.AspNetCore;
using System.Text;
using System.Security.Claims;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;

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
		ClockSkew = TimeSpan.Zero,
		NameClaimType = ClaimTypes.Name,
		RoleClaimType = ClaimTypes.Name
	};
});

builder.Services.AddApiVersioning(opt =>
{
	opt.AssumeDefaultVersionWhenUnspecified = true;
	opt.DefaultApiVersion = new ApiVersion(1, 0);
	opt.ReportApiVersions = true;
}).AddApiExplorer(opt =>
{
	opt.GroupNameFormat = "'v'VVV";
	opt.SubstituteApiVersionInUrl = true;
});
builder.Services.AddCors();

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(opt =>
{
	opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers();
var buildProvider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

foreach (var description in buildProvider.ApiVersionDescriptions)
{
	var versionName = description.GroupName;
	var versionNumber = description.ApiVersion.ToString();
	var displayName = $"Demo API --  {versionNumber}";
	builder.Services.AddOpenApi(versionName,opt =>
	{
		opt.AddDocumentTransformer((document, context, cancellationToken) =>
		{
			document.Info = new OpenApiInfo
			{
				Title = "Demo Royal API",
				Version = versionName,
				Description = displayName,
				Contact = new OpenApiContact
				{
					Name = "Dejan Jovanovic",
					Email = "dejanjovanovic@gmail.com"
				}
			};

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
}

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
/*builder.Services.AddOpenApi(opt =>
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
*/
builder.Services.AddAutoMapper(opt =>
{
	opt.CreateMap<Villa, VillaCreateDTO>().ReverseMap();
	opt.CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
	opt.CreateMap<Villa, VillaDTO>().ReverseMap();
	opt.CreateMap<VillaUpdateDTO, VillaDTO>().ReverseMap();
	opt.CreateMap<User, UserDTO>().ReverseMap();
	opt.CreateMap<VillaAmenities, VillaAmenitiesCreateDTO>().ReverseMap();
	opt.CreateMap<VillaAmenities, VillaAmenitiesUpdateDTO>().ReverseMap();
	opt.CreateMap<VillaAmenities, VillaAmenitiesDTO>()
	.ForMember(dest => dest.VillaName, opt => opt.MapFrom(src => src.Villa != null? src.Villa.Name : null));
	opt.CreateMap<VillaAmenitiesDTO, VillaAmenities>();
});
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();
await SeedData(app);
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi("openapi/{documentName}.json");
	var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();


	app.MapScalarApiReference(opt =>
	{
		opt.Title = "Demo - Royal Villa API";
		var sortedVersion = provider.ApiVersionDescriptions.OrderBy(v => v.ApiVersion).ToList();

		foreach (var description in sortedVersion)
		{
			var versionName = description.GroupName;
			var versionNumber = description.ApiVersion.ToString();
			var displayName = $"Demo API --  { versionNumber}";

			var isDefault = description.ApiVersion.Equals(new ApiVersion(2, 0));
			opt.AddDocument(versionName, displayName, $"/openapi/{versionName}.json", isDefault);
		}
	});
}

app.UseCors(o => o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("*"));
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