using Microsoft.EntityFrameworkCore;
using service1.Data;
using service1.Models;
using service1.Services;
using Serilog;
using Serilog.Formatting.Compact;

// настраиваем Serilog до создани€ builder
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// замен€ем стандартное логирование на Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console(new CompactJsonFormatter()));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DataGenerator>();
var runtimeConfig = new RuntimeConfig();
builder.Configuration.GetSection("Generator").Bind(runtimeConfig);
builder.Services.AddSingleton(runtimeConfig);
builder.Services.AddHostedService<GeneratorBackgroundService>();

// ѕодключаем базу данных
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// јвтоматически создаЄм таблицы при старте
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "—ервис упал при запуске");
}
finally
{
    Log.CloseAndFlush(); // убеждаемс€ что все логи записаны перед выходом
}