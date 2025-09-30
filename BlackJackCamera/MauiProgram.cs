using BlackJackCamera.Interfaces;
using BlackJackCamera.Services;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace BlackJackCamera
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitCamera()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services
            builder.Services.AddSingleton<IModelLoaderService, ModelLoaderService>();
            builder.Services.AddSingleton<IObjectDetectionService, ObjectDetectionService>();
            builder.Services.AddTransient<IImageProcessor, ImageProcessor>();

            // Register pages
            builder.Services.AddSingleton<App>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<CameraPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
