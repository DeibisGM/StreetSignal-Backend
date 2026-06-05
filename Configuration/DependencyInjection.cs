using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StreetSignalApi.Common.Services;
using StreetSignalApi.Data;
using StreetSignalApi.Models;
using StreetSignalApi.Permissions;
using StreetSignalApi.Repositories.Implementations;
using StreetSignalApi.Repositories.Interfaces;
using StreetSignalApi.Services.Implementations;
using StreetSignalApi.Services.Interfaces;

namespace StreetSignalApi.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddStreetSignal(this IServiceCollection services, IConfiguration config)
    {
        services.AddPersistence(config);
        services.AddJwtAuth(config);
        services.AddRepositories();
        services.AddDomainServices(config);
        services.AddApi();
        services.AddSwagger();
        return services;
    }

    private static void AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        var connString = config.GetConnectionString("Default")
            ?? "Data Source=streetsignal.db";

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseMySql(connString, ServerVersion.AutoDetect(connString)));
    }

    private static void AddJwtAuth(this IServiceCollection services, IConfiguration config)
    {
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.SectionName));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Configure JwtBearerOptions lazily so the correct values are used whether
        // running normally or inside WebApplicationFactory (which injects config overrides
        // after service registration time).
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOpts) =>
            {
                var jwt = jwtOpts.Value;
                var signingKey = string.IsNullOrWhiteSpace(jwt.SigningKey)
                    ? "dev-only-signing-key-please-override-32+chars"
                    : jwt.SigningKey;

                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                // Override default 401/403 bodies to match the API contract
                bearerOptions.Events = new JwtBearerEvents
                {
                    OnChallenge = ctx =>
                    {
                        ctx.HandleResponse();
                        return AuthResponseFactories.WriteUnauthorizedAsync(ctx.HttpContext);
                    },
                    OnForbidden = ctx => AuthResponseFactories.WriteForbiddenAsync(ctx.HttpContext)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.StaffOnly, p => p.RequireRole(Roles.Staff, Roles.Admin));
            options.AddPolicy(Policies.CitizenOnly, p => p.RequireRole(Roles.Citizen));
            options.AddPolicy(Policies.AuthenticatedUser, p => p.RequireAuthenticatedUser());
        });

        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
    }

    private static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IReportUpdateRepository, ReportUpdateRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
    }

    private static void AddDomainServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IReportUpdateService, ReportUpdateService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFileService, FileService>();
        services.AddFirebasePush(config);
    }

    private static void AddFirebasePush(this IServiceCollection services, IConfiguration config)
    {
        var credPath = config["Firebase:CredentialPath"];
        if (!string.IsNullOrWhiteSpace(credPath) && File.Exists(credPath))
        {
            if (FirebaseApp.DefaultInstance is null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credPath),
                });
            }
            services.AddScoped<IPushNotificationService, FirebasePushNotificationService>();
        }
        else
        {
            services.AddScoped<IPushNotificationService, NullPushNotificationService>();
        }
    }

    private static void AddApi(this IServiceCollection services)
    {
        services.AddControllers();

        // Replace the default ValidationProblemDetails with the contract's ValidationErrorResponse
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = ValidationProblemFilter.Build;
        });
    }

    private static void AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "StreetSignal Platform API",
                Version = "v1",
                Description = "Citizen reports platform API."
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT bearer token. Example: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
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
                    Array.Empty<string>()
                }
            });
        });
    }
}
