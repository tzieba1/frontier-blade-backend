using System.Text;
using System.Text.Json.Serialization;
using FbsApi.Data;
using FbsApi.Services;
using FbsApi.Services.Interfaces;
using FbsApi.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add MongoDB configuration
builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection("MongoDBSettings"));
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(builder.Configuration.GetValue<string>("MongoDBSettings:ConnectionString")));

// Repositories
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<TeamRepository>();
builder.Services.AddSingleton<EmployeeRepository>();
builder.Services.AddSingleton<ApprovalRepository>();
builder.Services.AddSingleton<TimeSheetRepository>();
// builder.Services.AddSingleton<TimeSheetEntryRepository>();
builder.Services.AddSingleton<RefreshTokenRepository>();

// Data Mappers
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// API Services
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddTransient<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, SendGridSender>();
builder.Services.AddTransient<FbsApi.Services.Interfaces.IEmailSender, GmailSender>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors();

// Configure Email Sending
builder.Services.Configure<EmailSenderOptions>(builder.Configuration.GetSection("EmailSenderOptions"));
builder.Services.Configure<EmailConfirmationUri>(builder.Configuration.GetSection("EmailConfirmationUri"));

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured"));
var tokenValidationParams = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    RequireExpirationTime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = jwtSettings["Issuer"],
    ValidAudience = jwtSettings["Audience"],
    IssuerSigningKey = new SymmetricSecurityKey(key),
};

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            //TODO: Add claims here
            return Task.CompletedTask;
        }
    };
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;   //TODO: Set to true in Production
    options.TokenValidationParameters = tokenValidationParams;
});

// Service for JWT Refresh Tokens
builder.Services.AddSingleton(tokenValidationParams);

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SalesManagerOnly", policy =>
        policy.RequireRole("Manager")
              .RequireClaim("Department", "Sales"));
});

// Add controllers and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()); // This converts enums to strings in JSON.
    });

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Frontier Blade Solutions API", Version = "v1" });

    // Map enums to strings in Swagger UI
    c.MapType<Role>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(Role)).Select(e => new OpenApiString(e)).Cast<IOpenApiAny>().ToList()
    });
    c.MapType<EmployeeType>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(EmployeeType)).Select(e => new OpenApiString(e)).Cast<IOpenApiAny>().ToList()
    });
    c.MapType<ApprovalStatus>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(ApprovalStatus)).Select(e => new OpenApiString(e)).Cast<IOpenApiAny>().ToList()
    });

    // Add JWT Bearer authorization to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field, for example: \"bearer {Value}\"",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

// Middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Controller endpoints
app.UseEndpoints(endpoints =>
    {
        _ = endpoints.MapControllers();
    }
);

app.Run();