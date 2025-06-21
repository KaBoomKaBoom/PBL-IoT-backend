using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// builder.WebHost.UseUrls("http://*:8080", "http://*:5000");


// Add the connection string from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Important for cookies or auth headers
    });
});



var Configuration = builder.Configuration;
// Console.WriteLine($"Database={Environment.GetEnvironmentVariable("PG_DATABASE")};Username={Environment.GetEnvironmentVariable("PG_USERNAME")};Password={Environment.GetEnvironmentVariable("PG_PASSWORD")}");
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseNpgsql(
//         $"Host=db;Port=5432;Database={Environment.GetEnvironmentVariable("PG_DATABASE")};Username={Environment.GetEnvironmentVariable("PG_USERNAME")};Password={Environment.GetEnvironmentVariable("PG_PASSWORD")}"
//     ));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        $"Host=db;Port=5432;Database=db;Username=admin;Password=admin"
    ).EnableSensitiveDataLogging()
        .EnableDetailedErrors()
        .LogTo(Console.WriteLine, LogLevel.Information)
    );

// Add MQTT service
builder.Services.AddHostedService<MqttSensorService>();

// Read JWT values directly
var jwtConfig = builder.Configuration.GetSection("JwtSettings");
var key = jwtConfig["Key"];
var issuer = jwtConfig["Issuer"];
var audience = jwtConfig["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        // ValidIssuer = issuer,
        // ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew = TimeSpan.Zero
    };
    // // Add this to your JWT bearer options in Program.cs
    // options.Events = new JwtBearerEvents
    // {
    //     OnMessageReceived = context =>
    //     {
    //         var authHeader = context.Request.Headers["Authorization"].ToString();
    //         Console.WriteLine($"Raw Authorization header: '{authHeader}'");

    //         if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
    //         {
    //             var token = authHeader.Substring("Bearer ".Length).Trim();
    //             Console.WriteLine($"Extracted token: '{token}'");
    //             Console.WriteLine($"Token length: {token.Length}");
    //         }
    //         else
    //         {
    //             Console.WriteLine("No valid Bearer token found in Authorization header");
    //         }

    //         return Task.CompletedTask;
    //     },
    //     OnAuthenticationFailed = context =>
    //     {
    //         Console.WriteLine("Authentication failed: " + context.Exception.Message);
    //         return Task.CompletedTask;
    //     },
    //     OnTokenValidated = context =>
    //     {
    //         Console.WriteLine("Token validated successfully");
    //         return Task.CompletedTask;
    //     },
    //     OnChallenge = context =>
    //     {
    //         Console.WriteLine("OnChallenge: " + context.Error);
    //         return Task.CompletedTask;
    //     }
    // };
});

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtService>(); // your own JwtService if you have one


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();
    const int maxRetries = 5;
    const int delaySeconds = 5;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation($"Attempt {attempt} of {maxRetries}: Applying EF Core migrations...");
            await context.Database.MigrateAsync();
            logger.LogInformation("Migrations applied successfully.");

            // Seed data after successful migration
            logger.LogInformation("Seeding database with mock data...");
            await MockData.SeedData(context);
            logger.LogInformation("Database seeding completed successfully.");
            break; // Exit loop on success
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Attempt {attempt} of {maxRetries}: Failed to apply database migrations.");
            if (attempt == maxRetries)
            {
                logger.LogError("All retry attempts failed. Aborting migration and seeding.");
                throw; // Rethrow the last exception after max retries
            }
            logger.LogInformation($"Waiting {delaySeconds} seconds before retrying...");
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
        }
    }
}

app.UseRouting();
// Configure the HTTP request pipeline.
app.UseCors("FrontendPolicy");
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});



app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

