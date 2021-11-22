using Serilog;

const string ApplicationName = "k8stest";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up {ApplicationName}", ApplicationName);

try
{
    var builder = WebApplication.CreateBuilder(args);


    if (File.Exists("/config/appsettings.json"))
    {
        // this is less than ideal, this file should load after the local appsettings.json, not at the end
        builder.Configuration.AddJsonFile("/config/appsettings.json", optional: true);
    }

    string instance = builder.Configuration.GetValue<string>("Instance") ?? "n/a";

    builder.Host.UseSerilog((hostBuilderContext, loggerConfiguration) => loggerConfiguration
       .ReadFrom.Configuration(hostBuilderContext.Configuration)
       .Enrich.WithProperty("Application", ApplicationName)
       .Enrich.WithProperty("Version", k8stest.Utility.Version.GetCurrent())
       .Enrich.WithProperty("Instance", instance)
       .Enrich.FromLogContext()
       .WriteTo.Console());

    string redisConfig = builder.Configuration.GetValue<string>("RedisConfiguration");

    if (string.IsNullOrEmpty(redisConfig))
    {
        builder.Services.AddDistributedMemoryCache();
    }
    else
    {
        string redisInstance = builder.Configuration.GetValue<string>("RedisInstance") ?? "k8stest";

        builder.Services.AddStackExchangeRedisCache(_ =>
        {
            _.Configuration = redisConfig;
            _.InstanceName = redisInstance;
        });
    }

    builder.Services.AddSession(_ =>
    {
        _.Cookie.HttpOnly = true;
        _.Cookie.IsEssential = true;
    });

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    var forwardedHeaders = !string.IsNullOrEmpty(builder.Configuration.GetValue<string>("UseForwardedHeaders"));

    var app = builder.Build();

    if(forwardedHeaders)
    {
        app.UseForwardedHeaders();
    }

    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
    }

    app.Use(async (context, next) =>
    {
        using (Serilog.Context.LogContext.PushProperty("TraceIdentifier", context.TraceIdentifier))
        {
            using (Serilog.Context.LogContext.PushProperty("RemoteAddress", context.Connection.RemoteIpAddress))
            {
                await next.Invoke();
            }
        }
    });

    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthorization();

    app.UseSession();

    app.Use(async (context, next) =>
    {
        if (!context.Session.Keys.Contains("CreatedAt"))
        {
            context.Session.Set("CreatedAt", BitConverter.GetBytes(DateTime.Now.Ticks));
            if (context.Connection.LocalIpAddress != null)
            {
                context.Session.Set("CreatedLocalIpAddress", context.Connection.LocalIpAddress.GetAddressBytes());
            }
            context.Session.Set("CreatedLocalPort", BitConverter.GetBytes(context.Connection.LocalPort));
            if (context.Connection.RemoteIpAddress != null)
            {
                context.Session.Set("CreatedRemoteIpAddress", context.Connection.RemoteIpAddress.GetAddressBytes());
            }
            context.Session.Set("CreatedRemoteIpPort", BitConverter.GetBytes(context.Connection.RemotePort));
        }
        await next.Invoke();
    });

    app.UseEndpoints(_ => { _.MapControllers(); });

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception in {ApplicationName}", ApplicationName);
}
finally
{
    Log.Information("Shutting down {ApplicationName}", ApplicationName);
    Log.CloseAndFlush();
}