using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UtilityHub360.Data;
using UtilityHub360.Services;
using UtilityHub360.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "UtilityHub360 API", 
        Version = "v1",
        Description = "Loan Management System API"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS - Fixed configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000", 
                "https://localhost:3000",
                "http://localhost:5000",
                "https://localhost:5000",
                "https://wh1479740.ispot.cc",
                "http://wh1479740.ispot.cc"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);

// Add OpenAI Settings
var openAISettings = builder.Configuration.GetSection("OpenAISettings").Get<OpenAISettings>() ?? new OpenAISettings();
builder.Services.AddSingleton(openAISettings);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IBillAnalyticsService, BillAnalyticsService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<ISavingsService, SavingsService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IIncomeSourceService, IncomeSourceService>();
builder.Services.AddScoped<IDisposableAmountService, DisposableAmountService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IChatService, ChatService>();

// Add Background Services
builder.Services.AddHostedService<BillReminderBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in both Development and Production for API testing
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "UtilityHub360 API v1");
    c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger route
});

// Only show detailed error pages in development
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Only use HTTPS redirection in production to avoid CORS preflight issues in development



if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Add CORS middleware - must be before UseAuthentication and UseAuthorization
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add a simple welcome endpoint
app.MapGet("/", () => "UtilityHub360 API is running! Visit /swagger for API documentation");

// Add health check endpoint
app.MapGet("/health", (ApplicationDbContext db) => 
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
    var serverInfo = connectionString.Contains("localhost") ? "Local Database (localhost\\SQLEXPRESS)" 
                   : connectionString.Contains("174.138.185.18") ? "LIVE Production Database (174.138.185.18)" 
                   : "Unknown Database";
    
    return new { 
        Status = "Healthy", 
        Environment = app.Environment.EnvironmentName,
        Timestamp = DateTime.UtcNow,
        DatabaseServer = serverInfo,
        ConnectionStringPrefix = connectionString.Substring(0, Math.Min(50, connectionString.Length)) + "..."
    };
});

app.Run();