using BlackJackCamera.Api.Interfaces;
using BlackJackCamera.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "BlackJackCamera API", Version = "v1" });
});

// Register ML services
builder.Services.AddSingleton<IModelLoaderService, ModelLoaderService>();
builder.Services.AddSingleton<IObjectDetectionService, ObjectDetectionService>();
builder.Services.AddScoped<IImageProcessor, ImageProcessor>();

// Configure CORS for mobile apps
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure file upload limits
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

var app = builder.Build();

// Initialize ML model on startup
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var detectionService = scope.ServiceProvider.GetRequiredService<IObjectDetectionService>();

    try
    {
        logger.LogInformation("Initializing object detection model...");
        await detectionService.InitializeAsync();
        logger.LogInformation("Object detection model initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize object detection model");
        throw;
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BlackJackCamera API v1"));
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
