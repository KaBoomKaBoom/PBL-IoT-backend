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

builder.Services.AddCors((options) =>
    {
        options.AddPolicy("DevCors", (corsBuilder) =>
            {
                corsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        options.AddPolicy("ProdCors", (corsBuilder) =>
            {
                corsBuilder.WithOrigins("https://myProductionSite.com")
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
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
    ));

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

//!!!!!!!!After the migration, you can comment this out or remove it. It is only for the first time to create the database and tables.!!!!!!!!

// Thread.Sleep(10000); // Wait for 10 seconds to ensure the database is ready
// using (var scope = app.Services.CreateScope())
// {
//     var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
//     db.Database.Migrate(); // This applies the migration at startup
// }

app.UseRouting();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

