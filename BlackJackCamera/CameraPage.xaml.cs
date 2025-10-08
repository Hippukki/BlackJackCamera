using BlackJackCamera.Interfaces;
using BlackJackCamera.Services;
using CommunityToolkit.Maui.Core;

namespace BlackJackCamera
{
    /// <summary>
    /// Страница камеры с детекцией объектов
    /// </summary>
    public partial class CameraPage : ContentPage
    {
        private readonly IDetectionApiService _detectionApiService;
        private readonly IImageCompressionService _imageCompressionService;
        private readonly ICameraProvider _cameraProvider;
        private bool _isFlashOn = false;

        /// <summary>
        /// Инициализирует новый экземпляр страницы CameraPage
        /// </summary>
        /// <param name="detectionApiService">Сервис API детекции объектов</param>
        /// <param name="imageCompressionService">Сервис сжатия изображений</param>
        /// <param name="cameraProvider">Провайдер камеры</param>
        public CameraPage(IDetectionApiService detectionApiService, IImageCompressionService imageCompressionService, ICameraProvider cameraProvider)
        {
            InitializeComponent();

            _detectionApiService = detectionApiService;
            _imageCompressionService = imageCompressionService;
            _cameraProvider = cameraProvider;

            BackgroundColor = Colors.Transparent;
        }

        protected async override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);

            await _cameraProvider.RefreshAvailableCameras(CancellationToken.None);

            cameraView.SelectedCamera = _cameraProvider.AvailableCameras
                .Where(c => c.Position == CameraPosition.Rear).FirstOrDefault();
        }

        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            base.OnNavigatedFrom(args);

            cameraView.MediaCaptured -= OnMediaCaptured;
            cameraView.Handler?.DisconnectHandler();
        }

        private async void OnMediaCaptured(object sender, MediaCapturedEventArgs e)
        {
            try
            {
                // Копируем поток в память для повторного использования
                var memoryStream = new MemoryStream();
                e.Media.Position = 0;
                await e.Media.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Создаем ImageSource для замороженного кадра
                var displayStream = new MemoryStream();
                await memoryStream.CopyToAsync(displayStream);
                displayStream.Position = 0;
                memoryStream.Position = 0;

                var imageSource = ImageSource.FromStream(() => displayStream);

                // Показываем замороженное изображение и лоадер
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Corner1.IsVisible = false;
                    Corner2.IsVisible = false;
                    Corner3.IsVisible = false;
                    Corner4.IsVisible = false;
                    HintPill.IsVisible = false;
                    FrozenImage.Source = imageSource;
                    FrozenImage.IsVisible = true;
                    DarkOverlay.IsVisible = true;
                    LoadingIndicator.IsVisible = true;
                    LoadingIndicator.IsRunning = true;
                    ShutterButton.IsEnabled = false;
                });

                // Обрабатываем фото асинхронно
                await Task.Run(() => ProcessPhoto(memoryStream));
            }
            catch(Exception ex)
            {
                await DisplayAlert("Ошибка", $"{ex.Message}", "OK");

                // Скрываем лоадер при ошибке
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    HideLoadingUI();
                });
            }
        }

        private async void OnCaptureButtonClicked(object sender, EventArgs e)
        {
            try
            {
                await cameraView.CaptureImage(CancellationToken.None);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка захвата", ex.Message, "OK");
            }
        }


        /// <summary>
        /// Обрабатывает сфотографированное изображение и выполняет детекцию объектов через API
        /// </summary>
        /// <param name="stream">Поток с изображением</param>
        private async void ProcessPhoto(Stream stream)
        {
            try
            {
                // Сжимаем изображение для оптимальной передачи по сети
                using var compressedStream = await _imageCompressionService.CompressImageAsync(stream, 1920, 1080, 75);

                // Отправляем на backend API
                var response = await _detectionApiService.DetectObjectsAsync(compressedStream);

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (!response.Success)
                    {
                        HideLoadingUI();
                        DisplayAlert("Ошибка", response.ErrorMessage ?? "Неизвестная ошибка", "OK");
                        return;
                    }

                    // Конвертируем DTO в локальные объекты Detection
                    var detections = response.Detections.Select(dto => new Detection
                    {
                        X = dto.X,
                        Y = dto.Y,
                        Width = dto.Width,
                        Height = dto.Height,
                        Confidence = dto.Confidence,
                        ClassId = dto.ClassId
                    }).ToList();

                    DisplayDetectionResults(detections);
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    HideLoadingUI();
                    await DisplayAlert("Ошибка", $"Не удалось обработать изображение: {ex.Message}", "OK");
                });
            }
        }

        /// <summary>
        /// Отображает результаты детекции пользователю
        /// </summary>
        /// <param name="detections">Список обнаруженных объектов</param>
        private async void DisplayDetectionResults(List<Detection> detections)
        {
            // Скрываем лоадер
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;

            if (detections.Count == 0)
            {
                HideLoadingUI();
                await DisplayAlert("Результат", "Объекты не обнаружены", "OK");
                return;
            }

            // Получаем бейджи для распознанных объектов
            var badges = CategoryBadgeMapper.GetBadgesForDetections(detections);

            if (badges == null || badges.Count == 0)
            {
                // Если нет бейджей для распознанных объектов, показываем обычный alert
                HideLoadingUI();

                var message = "Кажется у нас пока нет подходящих по смыслу услуг для распознанных объектов.";

                await DisplayAlert($"Упс!", message, "OK");
                return;
            }

            // Отображаем бейджи
            ShowBadges(badges);
        }

        /// <summary>
        /// Отображает бейджи на экране с анимацией Bottom Sheet
        /// </summary>
        /// <param name="badges">Список бейджей для отображения</param>
        private async void ShowBadges(List<CategoryBadgeMapper.Badge> badges)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] ShowBadges called with {badges.Count} badges");
            BadgesContainer.Children.Clear();

            foreach (var badge in badges)
            {
                var frame = new Frame
                {
                    CornerRadius = 18,
                    Padding = new Thickness(16, 8),
                    HasShadow = false,
                    Margin = new Thickness(6, 4),
                    HorizontalOptions = LayoutOptions.Start,
                    Opacity = 0.8
                };

                frame.Background = Color.FromArgb("#0B0B0C");

                var label = new Label
                {
                    Text = badge.Text,
                    TextColor = Colors.WhiteSmoke,
                    FontSize = 15,
                    FontAttributes = FontAttributes.Bold,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    BackgroundColor = Colors.Transparent
                };

                frame.Content = label;
                BadgesContainer.Children.Add(frame);
            }

            // Показываем Bottom Sheet с анимацией
            System.Diagnostics.Debug.WriteLine("[DEBUG] Setting BottomSheet.IsVisible = true");
            BottomSheet.IsVisible = true;
            ShutterButton.IsEnabled = false;

            // Сначала устанавливаем начальное положение (за экраном)
            BottomSheet.TranslationY = 800;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Initial TranslationY: {BottomSheet.TranslationY}");

            // Даем время для отрисовки элементов
            await Task.Delay(100);

            System.Diagnostics.Debug.WriteLine($"[DEBUG] BottomSheet bounds: {BottomSheet.Bounds}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG] BottomSheet Height: {BottomSheet.Height}");

            // Анимация выдвижения снизу вверх
            System.Diagnostics.Debug.WriteLine("[DEBUG] Starting animation TranslateTo(0, 0)");
            await BottomSheet.TranslateTo(0, 0, 400, Easing.CubicOut);
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Animation finished. TranslationY: {BottomSheet.TranslationY}");
        }

        /// <summary>
        /// Обработчик нажатия на затемнённую область - скрывает панель с бейджами
        /// </summary>
        private async void OnOverlayTapped(object sender, EventArgs e)
        {
            await HideBottomSheet();
        }

        /// <summary>
        /// Обработчик свайпа вниз - скрывает панель с бейджами
        /// </summary>
        private async void OnBottomSheetSwipedDown(object sender, SwipedEventArgs e)
        {
            await HideBottomSheet();
        }

        /// <summary>
        /// Скрывает Bottom Sheet с анимацией
        /// </summary>
        private async Task HideBottomSheet()
        {
            if (!BottomSheet.IsVisible)
                return;

            // Анимация скрытия вниз
            await BottomSheet.TranslateTo(0, 800, 300, Easing.CubicIn);

            // Скрываем все элементы
            HideLoadingUI();
        }

        /// <summary>
        /// Скрывает UI загрузки
        /// </summary>
        private void HideLoadingUI()
        {
            Corner1.IsVisible = true;
            Corner2.IsVisible = true;
            Corner3.IsVisible = true;
            Corner4.IsVisible = true;
            HintPill.IsVisible = true;
            FrozenImage.IsVisible = false;
            DarkOverlay.IsVisible = false;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            BottomSheet.IsVisible = false;
            BottomSheet.TranslationY = 800; // Сбрасываем позицию для следующей анимации
            BadgesContainer.Children.Clear();
            ShutterButton.IsEnabled = true;
        }


        private async void OnBackClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        private async void OnInfoClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Инфо", "Наведите камеру на объект и нажмите кнопку съемки для распознавания", "OK");
        }

        private async void OnGalleryClicked(object sender, EventArgs e)
        {
            await DisplayAlert("Галерея", "Открытие галереи (заглушка)", "OK");
        }

        /// <summary>
        /// Обработчик нажатия кнопки вспышки
        /// </summary>
        private void OnFlashClicked(object sender, EventArgs e)
        {
            _isFlashOn = !_isFlashOn;

            if (_isFlashOn)
            {
                cameraView.CameraFlashMode = CameraFlashMode.On;
                FlashButton.Source = "CameraPage/icon_flash_on.svg";
            }
            else
            {
                cameraView.CameraFlashMode = CameraFlashMode.Off;
                FlashButton.Source = "CameraPage/icon_flash_off.svg";
            }
        }

    }
}
