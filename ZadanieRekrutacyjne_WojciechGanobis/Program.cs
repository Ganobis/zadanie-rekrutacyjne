using Microsoft.EntityFrameworkCore;
using ZadanieRekrutacyjne_WojciechGanobis.Data;
using ZadanieRekrutacyjne_WojciechGanobis.Interfaces;
using ZadanieRekrutacyjne_WojciechGanobis.Services;

namespace ZadanieRekrutacyjne_WojciechGanobis;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


        // Add services to the container.
        builder.Services.AddHttpClient<IStackOverflowTagService, StackOverflowTagService>();
        builder.Services.AddScoped<ITagService, TagService>();

        builder.Services.AddControllers();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.UseInlineDefinitionsForEnums();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
        }

        app.Run();
    }
}