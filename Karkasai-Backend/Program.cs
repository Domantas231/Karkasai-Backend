using System.Text;
using FluentValidation;
using FluentValidation.Results;
using HabitTribe;
using HabitTribe.Auth;
using HabitTribe.Auth.Model;
using HabitTribe.Data;
using HabitTribe.Repositories;
using HabitTribe.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Extensions;
using SharpGrip.FluentValidation.AutoValidation.Endpoints.Results;
using SharpGrip.FluentValidation.AutoValidation.Shared.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>();

// CORS configuration
var devFrontendOrigins = "_devFrontendOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: devFrontendOrigins,
        policy =>
        {
            policy.WithOrigins("http://localhost:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// Add controllers
builder.Services.AddControllers();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddFluentValidationAutoValidation(configuration =>
{
    configuration.OverrideDefaultResultFactoryWith<ProblemDetailsResultFactory>();
});

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidAudience = builder.Configuration["Jwt:ValidAudience"],
        ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();

// Register Repositories
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

// Register Services
builder.Services.AddScoped<IGroupService, GroupService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ITagService, TagService>();

// Register Auth Services
builder.Services.AddTransient<IJwtTokenService, JwtTokenService>();
builder.Services.AddTransient<ISessionService, SessionService>();
builder.Services.AddScoped<AuthSeeder>();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    var dbSeeder = scope.ServiceProvider.GetRequiredService<AuthSeeder>();
    await dbSeeder.SeedAsync();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(devFrontendOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

namespace HabitTribe
{
    public class ProblemDetailsResultFactory : IFluentValidationAutoValidationResultFactory
    {
        public IResult CreateResult(EndpointFilterInvocationContext context, ValidationResult validationResult)
        {
            var problemDetails = new HttpValidationProblemDetails(validationResult.ToValidationProblemErrors())
            {
                Type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                Title = "Unprocessable Entity",
                Status = 422
            };
          
            return Results.Problem(problemDetails);
        }
    }
}