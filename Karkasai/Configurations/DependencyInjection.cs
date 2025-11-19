// using Microsoft.EntityFrameworkCore;
// using Karkasai.Data;
// using Karkasai.Entities;
// using Karkasai.Repositories;
// using Karkasai.Services;
//
// namespace Karkasai.Configurations;
//
// public static class DependencyInjection
// {
//     public static IServiceCollection AddProjectDependencies(
//         this IServiceCollection services, 
//         IConfiguration configuration)
//     {
//         services.AddDbContext<ApplicationDbContext>((_, builder) =>
//         {
//             var connectionString = configuration.GetConnectionString("DatabaseConnection");
//             builder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
//         });
//         
//         services.AddScoped<IRepository<Group>, GroupRepository>();
//         services.AddScoped<GroupService>();
//         
//         return services;
//     }
// }