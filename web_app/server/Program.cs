using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace server 
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<Database>(options => options.UseSqlite("Name=library.db"));
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseCors(CorsPolicyBuilder => 
                CorsPolicyBuilder.WithOrigins("*")
                .AllowAnyMethod()
                .AllowAnyHeader()
            );

            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}