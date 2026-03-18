using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using URLService.Algorithm;
using URLService.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Entity Framework Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 10,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null
        )
    )
    .ConfigureWarnings(warnings =>
        warnings.Ignore(RelationalEventId.PendingModelChangesWarning))
);

// 2. Add Redis Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    options.InstanceName = "URLShortener_";
});

// Register the Base62 Algorithm implementation
builder.Services.AddScoped<IURLShortenerAlgorithm, Base62ShortenerAlgorithm>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<URLService.RabbitMQ.UrlConsumer>();

var app = builder.Build();

// Retry EnsureCreated until SQL Server is ready
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    var retries = 10;
    while (retries > 0)
    {
        try
        {
            context.Database.Migrate();
            logger.LogInformation("Database ready.");
            break;
        }
        catch (Exception ex)
        {
            retries--;
            logger.LogWarning($"SQL Server not ready. Retrying... ({retries} left). Error: {ex.Message}");
            Thread.Sleep(5000);
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c => {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "URL Shortener API V1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();