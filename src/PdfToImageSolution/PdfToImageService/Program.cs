
using NLog;
using NLog.Web;
using PdfToImageService.Services;

internal class Program
{
    /// <summary>
    /// Main entry point.
    /// Application using NLog. This method is simple logging wrapper for CreateWebApplication method.
    /// </summary>
    /// <param name="args"></param>
    private static void Main(string[] args)
    {
        var logger = LogManager.Setup()
                           .LoadConfigurationFromAppSettings()
                           .GetCurrentClassLogger();

        logger.Info("Start");
        try
        {
            WebApplication app = CreateWebApplication(args);
            app.Run();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Stopped program because of exception");
        }
        finally
        {
            logger.Info("Finish");
            NLog.LogManager.Shutdown();
        }
    }

    static WebApplication CreateWebApplication(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Logging.ClearProviders();
        builder.Host.UseNLog();

        builder.Services.AddControllers();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddTransient<IPdfProcessor, PdfProcessor>();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseAuthorization(); 

        app.MapControllers();
        return app;
    }
}