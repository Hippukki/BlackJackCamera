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
        private readonly IObjectDetectionService _detectionService;
        private readonly IImageProcessor _imageProcessor;
        private readonly ICameraProvider _cameraProvider;
        private bool _isFlashOn = false;

        /// <summary>
        /// Инициализирует новый экземпляр страницы CameraPage
        /// </summary>
        /// <param name="detectionService">Сервис детекции объектов</param>
        /// <param name="imageProcessor">Сервис обработки изображений</param>
        public CameraPage(IObjectDetectionService detectionService, IImageProcessor imageProcessor, ICameraProvider cameraProvider)
        {
            InitializeComponent();

            _detectionService = detectionService;
            _imageProcessor = imageProcessor;

            InitializeServicesAsync();

            BackgroundColor = Colors.Transparent;
            _cameraProvider = cameraProvider;
        }

        /// <summary>
        /// Асинхронно инициализирует сервисы детекции
        /// </summary>
        private async void InitializeServicesAsync()
        {
            try
            {
                await _detectionService.InitializeAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка инициализации", ex.Message, "OK");
            }
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
        /// Обрабатывает сфотографированное изображение и выполняет детекцию объектов
        /// </summary>
        /// <param name="stream">Поток с изображением</param>
        private void ProcessPhoto(Stream stream)
        {
            var bitmap = _imageProcessor.ResizeImage(stream, 640, 640);
            var tensor = _imageProcessor.ConvertToTensor(bitmap);

            var detections = _detectionService.DetectObjects(tensor);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                DisplayDetectionResults(detections);
            });
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

                var labels = _detectionService.GetClassNames();
                var message = "Кажется у нас пока нет подходящих по смыслу услуг для: ";
                message += string.Join("\n", detections.Take(5).Select(d =>
                    $"{labels[d.ClassId]}"));

                if (detections.Count > 5)
                    message += $"\n\n... и ещё {detections.Count - 5} объектов";

                await DisplayAlert($"Упс!", message, "OK");
                return;
            }

            // Отображаем бейджи
            ShowBadges(badges);
        }

        /// <summary>
        /// Отображает бейджи на экране
        /// </summary>
        /// <param name="badges">Список бейджей для отображения</param>
        private void ShowBadges(List<CategoryBadgeMapper.Badge> badges)
        {
            BadgesContainer.Children.Clear();

            foreach (var badge in badges)
            {
                var frame = new Frame
                {
                    CornerRadius = 21,
                    Padding = new Thickness(12, 6),
                    HasShadow = false,
                    Margin = new Thickness(4, 4),
                    HorizontalOptions = LayoutOptions.Start,
                    Opacity = 0.9 // Небольшая прозрачность
                };

                // Определяем градиент в зависимости от типа бейджа
                LinearGradientBrush gradient;

                switch (badge.Type)
                {
                    case CategoryBadgeMapper.BadgeType.Primary:
                        // Сине-фиолетовый градиент для основных услуг
                        gradient = new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 0),
                            GradientStops = new GradientStopCollection
                            {
                                new GradientStop { Color = Color.FromArgb("#4A5FD9"), Offset = 0.0f }, // Синий
                                new GradientStop { Color = Color.FromArgb("#8B5CF6"), Offset = 1.0f }  // Фиолетовый
                            }
                        };
                        break;

                    case CategoryBadgeMapper.BadgeType.Discount:
                        // Красный → прозрачный для скидок
                        gradient = new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 0),
                            GradientStops = new GradientStopCollection
                            {
                                new GradientStop { Color = Color.FromArgb("#EF4444"), Offset = 0.0f }, // Красный
                                new GradientStop { Color = Color.FromArgb("#50EF4444"), Offset = 1.0f } // Прозрачный красный
                            }
                        };
                        break;

                    case CategoryBadgeMapper.BadgeType.Secondary:
                    default:
                        // Голубой → бирюзовый для остальных услуг
                        gradient = new LinearGradientBrush
                        {
                            StartPoint = new Point(0, 0),
                            EndPoint = new Point(1, 0),
                            GradientStops = new GradientStopCollection
                            {
                                new GradientStop { Color = Color.FromArgb("#06B6D4"), Offset = 0.0f }, // Голубой
                                new GradientStop { Color = Color.FromArgb("#14B8A6"), Offset = 1.0f }  // Бирюзовый
                            }
                        };
                        break;
                }

                frame.Background = gradient;

                var label = new Label
                {
                    Text = badge.Text,
                    TextColor = Colors.White,
                    FontSize = 20,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    BackgroundColor = Colors.Transparent
                };

                frame.Content = label;
                BadgesContainer.Children.Add(frame);
            }

            // Показываем бейджи
            BadgesScrollView.IsVisible = true;
            ShutterButton.IsEnabled = false;
        }

        /// <summary>
        /// Обработчик нажатия на затемнённую область - скрывает бейджи
        /// </summary>
        private void OnOverlayTapped(object sender, EventArgs e)
        {
            HideLoadingUI();
        }

        /// <summary>
        /// Скрывает UI загрузки
        /// </summary>
        private void HideLoadingUI()
        {
            Corner1.IsVisible = false;
            Corner2.IsVisible = false;
            Corner3.IsVisible = false;
            Corner4.IsVisible = false;
            HintPill.IsVisible = false;
            FrozenImage.IsVisible = false;
            DarkOverlay.IsVisible = false;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            BadgesScrollView.IsVisible = false;
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
