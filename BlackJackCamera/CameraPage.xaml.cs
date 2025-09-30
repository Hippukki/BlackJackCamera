using BlackJackCamera.Interfaces;
using CommunityToolkit.Maui.Core;
using static Microsoft.Maui.ApplicationModel.Permissions;

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
            HideLoadingUI();

            if (detections.Count == 0)
            {
                await DisplayAlert("Результат", "Объекты не обнаружены", "OK");
                return;
            }

            var labels = _detectionService.GetClassNames();
            var message = string.Join("\n", detections.Take(5).Select(d =>
                $"{labels[d.ClassId]} ({d.Confidence * 100:F1}%)"));

            if (detections.Count > 5)
                message += $"\n\n... и ещё {detections.Count - 5} объектов";

            await DisplayAlert($"Обнаружено объектов: {detections.Count}", message, "OK");
        }

        /// <summary>
        /// Скрывает UI загрузки
        /// </summary>
        private void HideLoadingUI()
        {
            FrozenImage.IsVisible = false;
            DarkOverlay.IsVisible = false;
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
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
                FlashButton.Source = "icon_flash_on.svg";
            }
            else
            {
                cameraView.CameraFlashMode = CameraFlashMode.Off;
                FlashButton.Source = "icon_flash_off.svg";
            }
        }

    }
}
