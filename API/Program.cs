using System.Security.Claims;
using System.Text;
using Application.Mapping;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Services --------------------

// MVC + ProblemDetails-ready JSON
builder.Services.AddControllers()
    .AddJsonOptions(_ => { /* enum as strings, etc., if you need */ });

// Swagger + JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Instruks API", Version = "v1" });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT as: Bearer {token}"
    };
    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtScheme, Array.Empty<string>() }
    });
});

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data source=../Infrastructure/db.db")
);

// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlServer(
//         builder.Configuration.GetConnectionString("Default")
//         ?? "Server=localhost,1433;Database=InstruksDb;User Id=root;Password=password;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
//     )
// );

// Validators & AutoMapper
builder.Services.AddValidatorsFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// DI: your layers
Application.DependencyResolver.DependencyResolverService.RegisterApplicationLayer(builder.Services);
Infrastructure.DependencyResolver.DependencyResolverService.RegisterInfrastructureLayer(builder.Services);

// Authorization policies (role → policy)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanReadInstruks", p =>
        p.RequireAssertion(ctx =>
            ctx.User.IsInRole("Doctor") || ctx.User.IsInRole("Nurse")));

    options.AddPolicy("CanManageInstruks", p =>
        p.RequireRole("Doctor"));
});

// JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddScoped<ITokenService, TokenService>();
var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwt is null) throw new InvalidOperationException("JwtSettings missing from configuration.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // set true in prod when HTTPS enabled end-to-end
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = signingKey,
        ClockSkew = TimeSpan.FromSeconds(30), // tighter than default 5 min
        ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
    };
});

// Current user accessor (for service-layer checks)
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

// CORS: single policy for Angular dev (tight headers/methods)
const string AllowAngular = "AllowAngular";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AllowAngular, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .WithHeaders("Authorization", "Content-Type");
        // If you switch to cookies: add .AllowCredentials() and configure SameSite on cookies.
    });
});

// Access current user & HTML sanitizer
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Application.Interfaces.Security.ICurrentUser, Infrastructure.Security.CurrentUser>();
builder.Services.AddScoped<Application.Interfaces.Content.IHtmlSanitizerService, Infrastructure.Content.HtmlSanitizerService>();

// Rate limiting (login & mutations)
builder.Services.AddRateLimiter(o =>
{
    o.AddFixedWindowLimiter("AuthLimiter", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });

    o.AddFixedWindowLimiter("MutationLimiter", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 60;
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

// -------------------- Pipeline --------------------

// Global exception handler with ProblemDetails (no stack traces)
app.UseExceptionHandler(errApp =>
{
    errApp.Run(async ctx =>
    {
        ctx.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = app.Environment.IsDevelopment()
                ? "See server logs for details."
                : "If the problem persists, contact support.",
            Instance = ctx.Request.Path
        };
        await ctx.Response.WriteAsJsonAsync(problem);
    });
});

// HSTS in production
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

// Minimal secure headers (CSP comes later when we wire Angular)
app.Use(async (ctx, next) =>
{
    var h = ctx.Response.Headers;
    h["X-Content-Type-Options"] = "nosniff";
    h["X-Frame-Options"] = "DENY";
    h["Referrer-Policy"] = "no-referrer";
    h["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    await next();
});

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Protect /swagger in prod (requires auth)
    app.MapWhen(ctx => ctx.Request.Path.StartsWithSegments("/swagger"), branch =>
    {
        branch.UseAuthentication();
        branch.UseAuthorization();
        branch.Use(async (ctx, next) =>
        {
            if (!ctx.User.Identity?.IsAuthenticated ?? true)
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            await next();
        });
        branch.UseSwagger();
        branch.UseSwaggerUI();
    });
}

app.UseRouting();

// CORS before authZ
app.UseCors(AllowAngular);

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();

app.Run();


// ----------------- Support types -----------------
public interface ICurrentUser
{
    string? UserId { get; }
    bool IsDoctor { get; }
    bool IsNurse { get; }
}

public sealed class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _http;
    public CurrentUser(IHttpContextAccessor http) => _http = http;

    public string? UserId => _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public bool IsDoctor => _http.HttpContext?.User.IsInRole("Doctor") == true;
    public bool IsNurse => _http.HttpContext?.User.IsInRole("Nurse") == true;
}
