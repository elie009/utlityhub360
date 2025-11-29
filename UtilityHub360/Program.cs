using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using UtilityHub360.Data;
using UtilityHub360.Services;
using UtilityHub360.Models;
using Microsoft.AspNetCore.Cors;

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
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
    {
        sqlOptions.CommandTimeout(120); // Increase command timeout to 120 seconds
    });
    options.EnableSensitiveDataLogging(false);
    options.EnableServiceProviderCaching();
});

// Add CORS - Allow specific origins with credentials
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "https://www.utilityhub360.com",
                "https://utilityhub360.com",
                "https://api.utilityhub360.com",
                "https://wh1479740.ispot.cc",
                "http://wh1479740.ispot.cc",
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:5000",
                "https://localhost:5000",
                "http://localhost:64653",
                "https://localhost:64653"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add JWT Authentication
var jwtSettings = new JwtSettings
{
    SecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    Issuer = builder.Configuration["JwtSettings:Issuer"] ?? "UtilityHub360",
    Audience = builder.Configuration["JwtSettings:Audience"] ?? "UtilityHub360Users",
    ExpirationMinutes = int.Parse(builder.Configuration["JwtSettings:ExpirationMinutes"] ?? "60")
};

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
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

// Add Response Caching
builder.Services.AddResponseCaching();

// Add Services
builder.Services.AddScoped<AccountingService>();
builder.Services.AddScoped<LoanAccountingService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ILoanService, LoanService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IEnhancedNotificationService, NotificationService>();
builder.Services.AddScoped<ISmsService, SmsService>();
builder.Services.AddScoped<IBillService, BillService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IBillAnalyticsService, BillAnalyticsService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddHttpContextAccessor(); // Required for AuditLogService
builder.Services.AddScoped<IUtilityService, UtilityService>();
builder.Services.AddScoped<IBankAccountService>(sp => 
{
    var context = sp.GetRequiredService<ApplicationDbContext>();
    var serviceProvider = sp;
    var accountingService = sp.GetRequiredService<AccountingService>();
    return new BankAccountService(context, serviceProvider, accountingService);
});
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IReceivableService, ReceivableService>();
builder.Services.AddScoped<ISavingsService, SavingsService>();
builder.Services.AddScoped<SavingsInterestCalculationService>();
builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IIncomeSourceService, IncomeSourceService>();
builder.Services.AddScoped<IDisposableAmountService, DisposableAmountService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<IOnboardingService, OnboardingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAllocationService, AllocationService>();
builder.Services.AddSingleton<IDocumentationSearchService, DocumentationSearchService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IAIAgentService>(sp =>
{
    var context = sp.GetRequiredService<ApplicationDbContext>();
    var bankAccountService = sp.GetRequiredService<IBankAccountService>();
    var logger = sp.GetRequiredService<ILogger<AIAgentService>>();
    var openAISettings = sp.GetRequiredService<OpenAISettings>();
    return new AIAgentService(context, bankAccountService, logger, openAISettings);
});
builder.Services.AddScoped<IFinancialReportService, FinancialReportService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IReconciliationService>(sp =>
{
    var context = sp.GetRequiredService<ApplicationDbContext>();
    var extractionService = sp.GetRequiredService<IBankStatementExtractionService>();
    var aiAgentService = sp.GetRequiredService<IAIAgentService>();
    var ocrService = sp.GetRequiredService<IOcrService>();
    var bankAccountService = sp.GetRequiredService<IBankAccountService>();
    var logger = sp.GetRequiredService<ILogger<ReconciliationService>>();
    var openAISettings = sp.GetRequiredService<OpenAISettings>();
    return new ReconciliationService(context, extractionService, aiAgentService, ocrService, bankAccountService, logger, openAISettings);
});
builder.Services.AddScoped<IBankStatementExtractionService>(sp =>
{
    var context = sp.GetRequiredService<ApplicationDbContext>();
    var aiAgentService = sp.GetRequiredService<IAIAgentService>();
    var ocrService = sp.GetRequiredService<IOcrService>();
    var logger = sp.GetRequiredService<ILogger<BankStatementExtractionService>>();
    var openAISettings = sp.GetRequiredService<OpenAISettings>();
    return new BankStatementExtractionService(context, aiAgentService, ocrService, logger, openAISettings);
});
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBankFeedService, BankFeedService>();
builder.Services.AddScoped<ITransactionRulesService, TransactionRulesService>();
builder.Services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
builder.Services.AddScoped<ISmartCategorizationService, SmartCategorizationService>();
builder.Services.AddScoped<ISpendingPatternService, SpendingPatternService>();
builder.Services.AddScoped<IAutomatedAlertsService, AutomatedAlertsService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddScoped<IReceiptService, ReceiptService>();
builder.Services.AddScoped<ITicketService, TicketService>();

// Add Background Services
builder.Services.AddHostedService<BillReminderBackgroundService>();
builder.Services.AddHostedService<BillPaymentSchedulingService>();
builder.Services.AddHostedService<BankAccountSyncBackgroundService>();
builder.Services.AddHostedService<SavingsInterestBackgroundService>();

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



// Configure CORS - must be before UseAuthentication and UseAuthorization
app.UseCors("AllowAll");

// Disable HTTPS redirection completely for local development
// app.UseHttpsRedirection(); // Commented out to prevent any HTTPS redirects

// Add Response Caching Middleware
app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


// Add a simple welcome endpoint
app.MapGet("/", () => "UtilityHub360 API is running! Visit /swagger for API documentation");

// Add a simple CORS test endpoint
app.MapGet("/test-cors", (HttpContext context) => 
{
    var origin = context.Request.Headers["Origin"].FirstOrDefault();
    return new { 
        message = "CORS Test", 
        origin = origin,
        timestamp = DateTime.UtcNow,
        environment = app.Environment.EnvironmentName
    };
});

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