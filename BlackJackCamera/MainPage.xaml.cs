namespace BlackJackCamera;

/// <summary>
/// Главная страница приложения
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Инициализирует новый экземпляр главной страницы
    /// </summary>
    /// <param name="serviceProvider">Провайдер сервисов для DI</param>
    public MainPage(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Обработчик нажатия кнопки сканирования
    /// </summary>
    private async void OnScanClicked(object sender, EventArgs e)
    {
        var cameraPage = _serviceProvider.GetRequiredService<CameraPage>();
        await Navigation.PushAsync(cameraPage);
    }
}