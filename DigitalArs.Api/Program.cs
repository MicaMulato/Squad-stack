using DigitalArs.Application.Interfaces;
using DigitalArs.Domain.Entities;
using DigitalArs.Infrastructure.Data;
using DigitalArs.Infrastructure.Repositories;
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
            // NUEVO: DbContext
            // ============================================================
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


            // NUEVO: Identity — habilita UserManager<User> / RoleManager<Role>
            builder.Services.AddIdentity<User, Role>(options =>
            {
                // Config mínima para desarrollo, ajustar cuando la situación lo requiera
                options.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // ============================================================
            // NUEVO: Repository genérico + Unit of Work
            // (solo para entidades que heredan BaseEntity: Account, Transaction, etc.
            //  User y Role se manejan con UserManager/RoleManager, no por acá)
            // ============================================================
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            // NUEVO: UseAuthentication SIEMPRE antes de UseAuthorization.
            // Sin esto, Identity/JWT nunca autentica al usuario (y no tira
            // error, simplemente falla silenciosamente en cada request).
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}