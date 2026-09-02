using DigitalArs.Application;
using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Infrastructure.Data;
using DigitalArs.Infrastructure.Repositories;
using DigitalArs.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DigitalArs
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // ============================================================
            // DbContext
            // ============================================================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity — habilita UserManager<User> / RoleManager<Role>
            builder.Services.AddIdentity<User, Role>(options =>
            {
                // Config minima para desarrollo, ajustar cuando la situacion lo requiera
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Servicio de hashing de contraseñas
            builder.Services.AddScoped<IPasswordHasher, PasswordHasherService>();

            // ============================================================
            // Repository generico + Unit of Work
            // (solo para entidades que heredan BaseEntity: Account, Transaction, etc.
            //  User y Role se manejan con UserManager/RoleManager, no por aca)
            // ============================================================
            builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddApplication(); // ← registra Mapster + FluentValidation

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // UseAuthentication SIEMPRE antes de UseAuthorization.
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
