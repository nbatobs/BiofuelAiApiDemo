using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OpenApi;
using Api.Services;
using Serilog;
using System. Reflection;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    Log.Information("Starting Biofuel AI API");

    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Add services to the container
    builder. Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Register application services
    builder. Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ISiteAuthorizationService, SiteAuthorizationService>();
    builder.Services.AddScoped<IDataIngestionService, DataIngestionService>();

    // Configure Microsoft Entra External ID Authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(options =>
        {
            builder.Configuration.Bind("BackendApi", options);
            options.TokenValidationParameters.NameClaimType = "name";
            
            // Configure the correct authority for CIAM
            options.Authority = $"https://{builder.Configuration["BackendApi:TenantId"]}.ciamlogin.com/{builder.Configuration["BackendApi:TenantId"]}/v2.0";
            
            options.TokenValidationParameters.ValidateIssuer = true;
            options.TokenValidationParameters.ValidIssuers = new[]
            {
                $"https://{builder.Configuration["BackendApi:TenantId"]}.ciamlogin.com/{builder.Configuration["BackendApi:TenantId"]}/v2.0"
            };
        },
        options => 
        {
            builder.Configuration.Bind("BackendApi", options);
        });

    // Configure Swagger/OpenAPI with OAuth2 authentication
    builder.Services.AddSwaggerGen(options =>
    {
        options.CustomSchemaIds(x => x.FullName);
        options.OrderActionsBy((apiDesc) => apiDesc.GroupName);
        
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Biofuel AI API",
            Version = "v1",
            Description = "API for Biofuel optimization and analytics",
            Contact = new OpenApiContact
            {
                Name = "Biofuel AI Team",
                Email = "support@biofuelai.com"
            }
        });

        // Include XML comments for better documentation (if available)
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }

        // Add OAuth2 authentication to Swagger
        options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
        {
            Description = "OAuth2 using Authorization Code Flow with Microsoft Entra External ID",
            Name = "OAuth2",
            Type = SecuritySchemeType.OAuth2,
            Flows = new OpenApiOAuthFlows
            {
                AuthorizationCode = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(
                        $"{builder.Configuration["SwaggerAuth:Instance"]}" +
                        $"{builder.Configuration["SwaggerAuth:TenantId"]}/oauth2/v2.0/authorize"
                    ),
                    TokenUrl = new Uri(
                        $"{builder.Configuration["SwaggerAuth:Instance"]}" +
                        $"{builder.Configuration["SwaggerAuth:TenantId"]}/oauth2/v2.0/token"
                    ),
                    Scopes = new Dictionary<string, string>
                    {
                        { 
                            $"api://{builder.Configuration["BackendApi:ClientId"]}/DemoApi.ReadWrite", 
                            "Read and write access to Biofuel AI API" 
                        }
                    }
                }
            }
        });

        // Apply OAuth2 security requirement globally
        options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("OAuth2", document)] = [] 
        });
    });

    // Configure PostgreSQL with Npgsql
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(
            connectionString,
            npgsqlOptions => npgsqlOptions.MigrationsAssembly("Data")));

    // Add CORS policy
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Biofuel AI API v1");
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            
            // Configure OAuth2 for Swagger UI
            options.OAuthClientId(builder.Configuration["SwaggerAuth:ClientId"]);
            options.OAuthUsePkce();
            options.OAuthScopeSeparator(" ");
        });
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");

    // Add Serilog request logging
    app. UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}