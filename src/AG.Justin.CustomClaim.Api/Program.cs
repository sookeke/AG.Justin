using AG.Justin.CustomClaim.Api.Infrastructure.Auth;
using AG.Justin.Infrastructure;
using FluentValidation.AspNetCore;
using Serilog;
using System.Reflection;
using System.Text.Json;
using AG.Justin.CustomClaim.Api.Features;
using Index = AG.Justin.CustomClaim.Api.Features.Participant.Index;
using Microsoft.AspNetCore.HttpLogging;
using AG.Justin.CustomClaim.Api.Extensions;

Log.Logger = new LoggerConfiguration()
    .Enrich.WithEnvironmentUserName()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();


builder.Services.AddHttpLogging(loggingOptions =>
{
    loggingOptions.LoggingFields = HttpLoggingFields.All;
});
//builder.Host.UseSerilog();

var config = InitializeConfiguration(builder.Services);

//builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

CustomClaimConfiguration InitializeConfiguration(IServiceCollection services)
{
    var config = new CustomClaimConfiguration();
    builder.Configuration.Bind(config);
    services.AddSingleton(config);

    Log.Logger.Information("### App Version:{0} ###", Assembly.GetExecutingAssembly().GetName().Version);
    Log.Logger.Information("### JUSTIN Participant Service Configuration:{0} ###", JsonSerializer.Serialize(config));

    return config;
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//implement custom claim dependency injection
builder.Services.AddCustomClaims(builder.Configuration);

//add keycloak service injection
builder.Services.AddKeycloakAuth(config);


//services.AddControllers(options => options.Conventions.Add(new RouteTokenTransformerConvention(new KabobCaseParameterTransformer())))
builder.Services.AddFluentValidationAutoValidation()
       .AddSingleton<Microsoft.Extensions.Logging.ILogger>(svc => svc.GetRequiredService<ILogger<Index.QueryHandler>>()); ;


//Log.Logger = new LoggerConfiguration()
//      .Enrich.WithEnvironmentUserName()
//      .WriteTo.Console()
//      .CreateLogger();

//builder.Services.AddLogging(loggingBuilder =>
//{
//    loggingBuilder.ClearProviders();
//    loggingBuilder.AddSerilog();
//});

//builder.Services.AddSingleton<DiagnosticContext>();

builder.Services.Scan(scan => scan
    .FromAssemblyOf<Program>()
    .AddClasses(classes => classes.AssignableTo<IRequestHandler>())
    .AsImplementedInterfaces()
    .WithTransientLifetime());

//builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging(options => options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
{
    var userId = httpContext.User.GetUserId();
    if (!userId.Equals(Guid.Empty))
    {
        diagnosticContext.Set("User", userId);
    }
});

app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
