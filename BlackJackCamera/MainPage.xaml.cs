namespace BlackJackCamera;

/// <summary>
/// Главная страница приложения
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly IServiceProvider _serviceProvider;
    private const string TooltipShownKey = "CameraTooltipShown";
    private bool _isPulsing = false;

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
    /// Вызывается при появлении страницы
    /// </summary>
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Проверяем, показывали ли уже подсказку
        var tooltipShown = Preferences.Default.Get(TooltipShownKey, false);

        if (!tooltipShown)
        {
            // Задержка перед показом подсказки
            await Task.Delay(800);
            await ShowTooltipAsync();
        }
    }

    /// <summary>
    /// Показывает подсказку с анимацией
    /// </summary>
    private async Task ShowTooltipAsync()
    {
        SpotlightOverlay.IsVisible = true;
        SpotlightOverlay.Opacity = 0;
        TooltipContainer.IsVisible = true;
        TooltipContainer.Opacity = 0;

        // Устанавливаем начальные позиции для анимации
        TooltipBadge.TranslationX = -50;
        TooltipBadge.Opacity = 0;
        ScannerButtonOverlay.Scale = 0.8;
        ScannerButtonOverlay.Opacity = 0;

        // Анимация появления overlay
        await SpotlightOverlay.FadeTo(1, 300, Easing.CubicOut);

        // Показываем контейнер
        await TooltipContainer.FadeTo(1, 100);

        // Анимация появления бейджа слева направо
        var badgeAnimation = TooltipBadge.FadeTo(1, 400, Easing.CubicOut);
        var badgeSlide = TooltipBadge.TranslateTo(0, 0, 400, Easing.CubicOut);

        await Task.WhenAll(badgeAnimation, badgeSlide);

        // Анимация появления кнопки
        await Task.WhenAll(
            ScannerButtonOverlay.FadeTo(1, 300, Easing.CubicOut),
            ScannerButtonOverlay.ScaleTo(1, 300, Easing.CubicOut)
        );

        // Запускаем пульсацию кнопки
        StartPulseAnimation();
    }

    /// <summary>
    /// Запускает анимацию пульсации кнопки сканера
    /// </summary>
    private async void StartPulseAnimation()
    {
        _isPulsing = true;

        while (_isPulsing && ScannerButtonOverlay.IsVisible)
        {
            // Пульсация: увеличение
            await ScannerButtonOverlay.ScaleTo(1.15, 800, Easing.SinInOut);

            // Пульсация: уменьшение
            await ScannerButtonOverlay.ScaleTo(1.0, 800, Easing.SinInOut);
        }
    }

    /// <summary>
    /// Обработчик нажатия на затемнённую область
    /// </summary>
    private async void OnSpotlightTapped(object sender, EventArgs e)
    {
        await HideTooltipAsync();
    }

    /// <summary>
    /// Скрывает подсказку с анимацией
    /// </summary>
    private async Task HideTooltipAsync()
    {
        _isPulsing = false;

        // Сохраняем, что подсказка была показана
        Preferences.Default.Set(TooltipShownKey, true);

        // Анимация исчезновения
        await Task.WhenAll(
            TooltipContainer.FadeTo(0, 250, Easing.CubicIn),
            SpotlightOverlay.FadeTo(0, 300, Easing.CubicIn)
        );

        SpotlightOverlay.IsVisible = false;
        TooltipContainer.IsVisible = false;
    }

    /// <summary>
    /// Обработчик нажатия кнопки сканирования
    /// </summary>
    private async void OnScanClicked(object sender, EventArgs e)
    {
        // Если подсказка активна, скрываем её
        if (SpotlightOverlay.IsVisible)
        {
            await HideTooltipAsync();
        }

        var cameraPage = _serviceProvider.GetRequiredService<CameraPage>();
        await Navigation.PushAsync(cameraPage);
    }
}