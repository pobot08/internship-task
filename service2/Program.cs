using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Formatting.Compact;
using service2.Data;
using service2.Services;
using service2.Workers;


namespace service2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(new CompactJsonFormatter())
                .CreateLogger();

            try
            {
                Log.Information("Запуск service2...");

                var builder = WebApplication.CreateBuilder(args);

                // Подключаем Serilog
                builder.Host.UseSerilog();

                builder.Services.AddDbContext<AppDbContext>(opt =>
                    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

                builder.Services.AddSingleton<Service3Client>();
                builder.Services.AddSingleton<RabbitMqPublisher>();

                builder.Services.AddHostedService<PollingWorker>();
                builder.Services.AddHostedService<DatabaseWorker>();
                builder.Services.AddHostedService<ExcelWorker>();

                builder.Services.AddHealthChecks()
                    .AddDbContextCheck<AppDbContext>("database");

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen();

                var app = builder.Build();

                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI();
                }

                app.UseHttpsRedirection();
                app.UseAuthorization();
                app.MapControllers();

                app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

                app.MapGet("/ready", async (AppDbContext db) =>
                {
                    try
                    {
                        await db.Database.CanConnectAsync();
                        return Results.Ok(new { status = "ready" });
                    }
                    catch
                    {
                        return Results.StatusCode(503);
                    }
                });

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Сервис упал при запуске");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}