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

        // Устанавливаем начальные позиции для анимации
        TooltipBadge.TranslationY = -50;
        TooltipBadge.Opacity = 0;
        ArrowPointer.Opacity = 0;
        PulseRing.Scale = 0.8;
        PulseRing.Opacity = 0;

        // Анимация появления overlay
        await SpotlightOverlay.FadeTo(1, 300, Easing.CubicOut);

        // Анимация появления бейджа
        var badgeAnimation = TooltipBadge.FadeTo(1, 400, Easing.CubicOut);
        var badgeSlide = TooltipBadge.TranslateTo(0, 0, 400, Easing.CubicOut);

        await Task.WhenAll(badgeAnimation, badgeSlide);

        // Анимация появления стрелки
        await ArrowPointer.FadeTo(0.9, 300, Easing.CubicOut);

        // Анимация появления кольца
        await Task.WhenAll(
            PulseRing.FadeTo(0.7, 300, Easing.CubicOut),
            PulseRing.ScaleTo(1, 300, Easing.CubicOut)
        );

        // Запускаем пульсацию кольца
        StartPulseAnimation();
    }

    /// <summary>
    /// Запускает анимацию пульсации кольца
    /// </summary>
    private async void StartPulseAnimation()
    {
        _isPulsing = true;

        while (_isPulsing && PulseRing.IsVisible)
        {
            await Task.WhenAll(
                PulseRing.ScaleTo(1.15, 800, Easing.SinInOut),
                PulseRing.FadeTo(0.3, 800, Easing.SinInOut)
            );

            await Task.WhenAll(
                PulseRing.ScaleTo(1, 800, Easing.SinInOut),
                PulseRing.FadeTo(0.7, 800, Easing.SinInOut)
            );
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
            TooltipBadge.FadeTo(0, 250, Easing.CubicIn),
            ArrowPointer.FadeTo(0, 250, Easing.CubicIn),
            PulseRing.FadeTo(0, 250, Easing.CubicIn),
            SpotlightOverlay.FadeTo(0, 300, Easing.CubicIn)
        );

        SpotlightOverlay.IsVisible = false;
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