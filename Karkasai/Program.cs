using Karkasai.Data;
using Karkasai.Entities;
using Karkasai.Repositories;
using Karkasai.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var  DevFrontendOrigins = "_devFrontendOrigins";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: DevFrontendOrigins,
        policy  =>
        {
            policy.WithOrigins("http://localhost:5173");
        });
});


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), 
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")))
);

builder.Services.AddScoped<IRepository<Group>, GroupRepository>();
builder.Services.AddScoped<GroupService>();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseCors(DevFrontendOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();