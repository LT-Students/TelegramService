using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using AspNetCoreRateLimit;
using DigitalOffice.Kernel.RedisSupport.Extensions;
using HealthChecks.UI.Client;
using LT.DigitalOffice.Kernel.BrokerSupport.Configurations;
using LT.DigitalOffice.Kernel.BrokerSupport.Extensions;
using LT.DigitalOffice.Kernel.BrokerSupport.Middlewares.Token;
using LT.DigitalOffice.Kernel.Configurations;
using LT.DigitalOffice.Kernel.EFSupport.Extensions;
using LT.DigitalOffice.Kernel.EFSupport.Helpers;
using LT.DigitalOffice.Kernel.Extensions;
using LT.DigitalOffice.Kernel.Middlewares.ApiInformation;
using LT.DigitalOffice.Kernel.RedisSupport.Configurations;
using LT.DigitalOffice.Kernel.RedisSupport.Constants;
using LT.DigitalOffice.Kernel.RedisSupport.Helpers;
using LT.DigitalOffice.TelegramService.Broker.Configurations;
using LT.DigitalOffice.TelegramService.Business.Features.Bot;
using LT.DigitalOffice.TelegramService.Business.Shared;
using LT.DigitalOffice.TelegramService.DataLayer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;

namespace LT.DigitalOffice.TelegramService;

public class Startup : BaseApiInfo
{
  public const string CorsPolicyName = "LtDoCorsPolicy";
  private string redisConnStr;

  private readonly RabbitMqConfig _rabbitMqConfig;
  private readonly BaseServiceInfoConfig _serviceInfoConfig;

  public IConfiguration Configuration { get; }

  public Startup(IConfiguration configuration)
  {
    Configuration = configuration;

    _serviceInfoConfig = Configuration
      .GetSection(BaseServiceInfoConfig.SectionName)
      .Get<BaseServiceInfoConfig>();

    _rabbitMqConfig = Configuration
      .GetSection(BaseRabbitMqConfig.SectionName)
      .Get<RabbitMqConfig>();

    Version = "1.0";
    Description = "TelegramService is an API that intended to work with Telegram.";
    StartTime = DateTime.UtcNow;
    ApiName = $"LT Digital Office - {_serviceInfoConfig.Name}";
  }

  public void ConfigureServices(IServiceCollection services)
  {
    services.AddCors(options =>
    {
      options.AddPolicy(
        CorsPolicyName,
        builder =>
        {
          builder
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
    });

    if (int.TryParse(Environment.GetEnvironmentVariable("RedisCacheLiveInMinutes"), out int redisCacheLifeTime))
    {
      services.Configure<RedisConfig>(options =>
      {
        options.CacheLiveInMinutes = redisCacheLifeTime;
      });
    }
    else
    {
      services.Configure<RedisConfig>(Configuration.GetSection(RedisConfig.SectionName));
    }

    services.Configure<TokenConfiguration>(Configuration.GetSection("CheckTokenMiddleware"));
    services.Configure<BaseRabbitMqConfig>(Configuration.GetSection(BaseRabbitMqConfig.SectionName));
    services.Configure<BaseServiceInfoConfig>(Configuration.GetSection(BaseServiceInfoConfig.SectionName));

    services.AddHttpContextAccessor();
    services.AddMemoryCache();

    services.Configure<IpRateLimitOptions>(options =>
      Configuration.GetSection("IpRateLimitingSettings").Bind(options));

    services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

    services.AddInMemoryRateLimiting();

    services
      .AddControllers()
      .AddJsonOptions(options =>
      {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
      })
      .AddNewtonsoftJson();

    string dbConnStr = ConnectionStringHandler.Get(Configuration);

    services.AddDbContext<TelegramServiceDbContext>(options =>
    {
      options.UseSqlServer(dbConnStr);
    });

    services.AddHealthChecks()
      .AddRabbitMqCheck()
      .AddSqlServer(dbConnStr);

    if (int.TryParse(Environment.GetEnvironmentVariable("MemoryCacheLiveInMinutes"), out int memoryCacheLifetime))
    {
      services.Configure<MemoryCacheConfig>(options =>
      {
        options.CacheLiveInMinutes = memoryCacheLifetime;
      });
    }
    else
    {
      services.Configure<MemoryCacheConfig>(Configuration.GetSection(MemoryCacheConfig.SectionName));
    }

    redisConnStr = services.AddRedisSingleton(Configuration);

    services.AddBusinessObjects();

    services.ConfigureMassTransit(_rabbitMqConfig);

    string botToken = Environment.GetEnvironmentVariable("BotToken");
    services.Configure<TelegramBotConfig>(configuration =>
    {
      configuration.BotHostAddress = Environment.GetEnvironmentVariable("BotHostAddress");
      configuration.BotRoute = Environment.GetEnvironmentVariable("BotRoute");
      configuration.BotToken = botToken;
      configuration.BotSecretToken = Environment.GetEnvironmentVariable("BotSecretToken");
    });

    services.AddHttpClient("TelegramBotClient")
      .AddTypedClient<ITelegramBotClient>(httpClient =>
      {
        TelegramBotClientOptions options = new(botToken);
        return new TelegramBotClient(options, httpClient);
      });

    services.AddScoped<UpdateHandler>();
    services.AddHostedService<ConfigureWebhook>();
  }

  public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
  {
    app.UpdateDatabase<TelegramServiceDbContext>();

    FlushRedisDbHelper.FlushDatabase(redisConnStr, Cache.Departments);

    app.UseForwardedHeaders();

    app.UseExceptionsHandler(loggerFactory);

    app.UseApiInformation();

    app.UseRouting();

    app.UseMiddleware<TokenMiddleware>();

    app.UseCors(CorsPolicyName);

    app.UseIpRateLimiting();

    app.UseEndpoints(endpoints =>
    {
      endpoints.MapControllers().RequireCors(CorsPolicyName);

      endpoints.MapHealthChecks("/hc",
        new HealthCheckOptions
        {
          ResultStatusCodes = new Dictionary<HealthStatus, int>
          {
            { HealthStatus.Unhealthy, 503 }, { HealthStatus.Healthy, 200 }, { HealthStatus.Degraded, 200 },
          },
          Predicate = check => check.Name != "masstransit-bus",
          ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });
    });
  }
}
