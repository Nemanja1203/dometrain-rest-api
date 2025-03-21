﻿using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Api.Endpoints;
using Movies.Api.Health;
using Movies.Api.Mapping;
using Movies.Api.Swagger;
using Movies.Application;
using Movies.Application.Database;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// TODO: (nm) Set serilog as default logging framework

// Add services to the container.

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
        ValidateIssuerSigningKey = true, // This is the main thing, without it nothing will be validated
        ValidateLifetime = true,
        ValidateIssuer = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = config["Jwt:Audience"]
    };
});

builder.Services.AddAuthorization(x =>
{
    // Role based
    // x.AddPolicy("AdminRole", p => p.RequireRole("Admin"));
    
    // x.AddPolicy(AuthConstants.AdminUserPolicyName,
    //     p => p.RequireClaim(AuthConstants.AdminUserClaimName, "true"));
    
    x.AddPolicy(AuthConstants.AdminUserPolicyName,
        p => p.AddRequirements(new AdminAuthRequirement(config["ApiKey"]!)));
    x.AddPolicy(AuthConstants.TrustedMemberPolicyName,
        p => p.RequireAssertion(c =>
            c.User.HasClaim(m => m is { Type: AuthConstants.AdminUserClaimName, Value: "true" }) ||
            c.User.HasClaim(m => m is { Type: AuthConstants.TrustedMemberClaimName, Value: "true" })));
});

builder.Services.AddScoped<ApiKeyAuthFilter>();

builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1.0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
    // x.ApiVersionReader = new HeaderApiVersionReader("api-version");
    x.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
}).AddApiExplorer();

builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddResponseCaching(); // Response caching - caching on client side
// TODO: (nm) Make output caching use Redis
builder.Services.AddOutputCache(x =>
{
    x.AddBasePolicy(c => c.Cache());
    x.AddPolicy("MovieCache", c => 
        c.Cache()
        .Expire(TimeSpan.FromMinutes(1))
        .SetVaryByQuery(new []{"title", "year", "sortBy", "page", "pageSize"})
        .Tag("movies")); // Tag is important as it allows us to do cache invalidation
});

// builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name);

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
// builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x => x.OperationFilter<SwaggerDefaultValues>());


//builder.Services.AddSingleton<IMovieRepository, MovieRepository>();
builder.Services.AddApplication();
builder.Services.AddDatabase(config["Database:ConnectionString"]!);

var app = builder.Build();

app.CreateApiVersionSet();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(x =>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            x.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                description.GroupName);
        }
    });
}

app.MapHealthChecks("_health");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//app.UseCors(); // Cors needs to be before response caching
// app.UseResponseCaching();

// Output Cache Defaults:
// - only 200 OK is cached
// - only GET and HEAD are cached
// - responses that set cookies are not cached
// - responses to authenticated requests are not cached
app.UseOutputCache(); 

app.UseMiddleware<ValidationMappingMiddleware>(); // Add before Controllers/ApiEndpoints
// app.MapControllers();
app.MapApiEndpoints();

var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
await dbInitializer.InitializeAsync();

app.Run();
