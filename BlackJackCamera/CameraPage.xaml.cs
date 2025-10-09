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

        // Кредитный оффер
        private int _currentStep = 0;
        private int _maxCreditAmount = 136000;
        private int _selectedCreditAmount = 75000;
        private readonly int[] _creditAmounts = new[] { 85000, 120000, 136000, 98000, 150000, 110000 };

        // Рассрочка
        private int _installmentCurrentStep = 0;
        private int _selectedPhonePrice = 89990;
        private readonly int[] _phonePrices = new[] { 89990, 119990, 74990, 99990, 134990, 149990 };
        private int _selectedInstallmentDuration = 0; // 6, 12, 24 месяцев

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
                        ClassId = dto.ClassId,
                        ClassName = dto.ClassName
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

            System.Diagnostics.Debug.WriteLine($"[DEBUG] DisplayDetectionResults: {detections.Count} detections found");
            foreach (var detection in detections)
            {
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Detection: ClassId={detection.ClassId}, ClassName={detection.ClassName}");
            }

            // Проверяем, есть ли категория "Ноутбук" среди распознанных объектов
            var categories = GetCategoriesFromDetections(detections);

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Categories found: {string.Join(", ", categories)}");

            if (categories.Contains("Ноутбук"))
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Laptop detected - showing credit offer");
                // Показываем кредитный оффер вместо обычных бейджей
                ShowCreditOffer();
                return;
            }

            if (categories.Contains("Телефон"))
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] Phone detected - showing installment offer");
                // Показываем оффер рассрочки вместо обычных бейджей
                ShowInstallmentOffer();
                return;
            }

            // Получаем бейджи для распознанных объектов
            var badges = CategoryBadgeMapper.GetBadgesForDetections(detections);

            if (badges == null || badges.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] No badges found for detections");
                // Если нет бейджей для распознанных объектов, показываем обычный alert
                HideLoadingUI();

                var message = "Кажется у нас пока нет услуг для: ";

                foreach (var detectClass in detections)
                    message += $"{detectClass.ClassName}, ";

                await DisplayAlert($"Упс!", message, "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Found {badges.Count} badges, calling ShowBadges");
            // Отображаем бейджи
            ShowBadges(badges);
        }

        /// <summary>
        /// Получает список категорий из распознанных объектов
        /// </summary>
        /// <param name="detections">Список распознанных объектов</param>
        /// <returns>Набор уникальных категорий</returns>
        private HashSet<string> GetCategoriesFromDetections(List<Detection> detections)
        {
            var categories = new HashSet<string>();

            // Используем рефлексию для доступа к приватному словарю маппинга
            var mapperType = typeof(CategoryBadgeMapper);
            var classToCategory = mapperType.GetField("_classToCategory",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?
                .GetValue(null) as Dictionary<int, string>;

            if (classToCategory != null)
            {
                foreach (var detection in detections)
                {
                    if (classToCategory.TryGetValue(detection.ClassId, out var category))
                    {
                        categories.Add(category);
                    }
                }
            }

            return categories;
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
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Creating badge: {badge.Text}, Type: {badge.Type}");

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

        #region Credit Offer

        /// <summary>
        /// Показывает кредитный оффер для категории "Ноутбук"
        /// </summary>
        private async void ShowCreditOffer()
        {
            // Выбираем случайную сумму кредита
            var random = new Random();
            _maxCreditAmount = _creditAmounts[random.Next(_creditAmounts.Length)];
            _selectedCreditAmount = _maxCreditAmount / 2;

            // Обновляем UI
            OfferTextLabel.Text = $"Вам предодобрен кредит на покупку ноутбука на сумму до {_maxCreditAmount:N0} ₽";
            AmountSlider.Maximum = _maxCreditAmount;
            AmountSlider.Value = _selectedCreditAmount;
            MaxAmountLabel.Text = $"{_maxCreditAmount:N0} ₽";
            SelectedAmountLabel.Text = $"{_selectedCreditAmount:N0} ₽";

            UpdateMonthlyPayment();

            // Показываем модальное окно с анимацией
            CreditOfferModal.IsVisible = true;
            CreditOfferModal.TranslationY = 800;
            await Task.Delay(50);

            // Сбрасываем на первый шаг ПОСЛЕ показа модального окна
            _currentStep = 0;
            ShowStep(0);
            UpdateStepIndicators();
            CreditActionButton.Text = "Выбрать сумму";

            await CreditOfferModal.TranslateTo(0, 0, 400, Easing.CubicOut);
        }

        /// <summary>
        /// Обработчик изменения значения слайдера
        /// </summary>
        private void OnAmountChanged(object sender, ValueChangedEventArgs e)
        {
            _selectedCreditAmount = (int)Math.Round(e.NewValue / 1000) * 1000; // Округляем до тысяч
            SelectedAmountLabel.Text = $"{_selectedCreditAmount:N0} ₽";
            UpdateMonthlyPayment();
        }

        /// <summary>
        /// Обновляет отображение ежемесячного платежа
        /// </summary>
        private void UpdateMonthlyPayment()
        {
            const double rate = 0.149; // 14.9% годовых
            const int months = 24;

            double monthlyRate = rate / 12;
            double monthlyPayment = _selectedCreditAmount * (monthlyRate * Math.Pow(1 + monthlyRate, months)) / (Math.Pow(1 + monthlyRate, months) - 1);

            MonthlyPaymentLabel.Text = $"≈ {monthlyPayment:N0} ₽";
        }

        /// <summary>
        /// Обработчик нажатия кнопки действия (переключение шагов)
        /// </summary>
        private async void OnCreditActionButtonClicked(object sender, EventArgs e)
        {
            if (_currentStep == 0)
            {
                // Шаг 1 -> Шаг 2 (Выбор суммы)
                _currentStep = 1;
                ShowStep(1);
                UpdateStepIndicators();
                CreditActionButton.Text = "Отправить заявку";
            }
            else if (_currentStep == 1)
            {
                // Шаг 2 -> Шаг 3 (Подтверждение)
                _currentStep = 2;
                ShowStep(2);
                UpdateStepIndicators();
                CreditActionButton.Text = "OK";

                // Генерируем номер заявки
                var applicationNumber = $"#2024-{random.Next(10000000, 99999999)}";
                ApplicationNumberLabel.Text = applicationNumber;
                FinalAmountLabel.Text = $"{_selectedCreditAmount:N0} ₽";
            }
            else if (_currentStep == 2)
            {
                // Шаг 3 -> Закрыть модальное окно
                await HideCreditOffer();
            }
        }

        /// <summary>
        /// Показывает указанный шаг и скрывает остальные
        /// </summary>
        private void ShowStep(int stepIndex)
        {
            Step1Content.IsVisible = stepIndex == 0;
            Step2Content.IsVisible = stepIndex == 1;
            Step3Content.IsVisible = stepIndex == 2;
        }

        /// <summary>
        /// Обновляет индикаторы шагов
        /// </summary>
        private void UpdateStepIndicators()
        {
            var color1 = _currentStep == 0 ? Color.FromArgb("#EF3124") : Color.FromArgb("#2B2B2D");
            var color2 = _currentStep == 1 ? Color.FromArgb("#EF3124") : Color.FromArgb("#2B2B2D");
            var color3 = _currentStep == 2 ? Color.FromArgb("#EF3124") : Color.FromArgb("#2B2B2D");

            // Принудительно обновляем через Dispatcher на UI потоке
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Step1Indicator.BackgroundColor = color1;
                Step2Indicator.BackgroundColor = color2;
                Step3Indicator.BackgroundColor = color3;

                // Принудительная перерисовка
                Step1Indicator.InvalidateMeasure();
                Step2Indicator.InvalidateMeasure();
                Step3Indicator.InvalidateMeasure();
            });
        }

        /// <summary>
        /// Скрывает кредитный оффер
        /// </summary>
        private async Task HideCreditOffer()
        {
            await CreditOfferModal.TranslateTo(0, 800, 300, Easing.CubicIn);
            CreditOfferModal.IsVisible = false;

            // Сбрасываем UI камеры
            HideLoadingUI();
        }

        private Random random = new Random();

        #endregion

        #region Installment Offer (Рассрочка)

        /// <summary>
        /// Показывает оффер рассрочки
        /// </summary>
        private async void ShowInstallmentOffer()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] ShowInstallmentOffer started");

                // Выбираем случайную цену телефона
                var random = new Random();
                _selectedPhonePrice = _phonePrices[random.Next(_phonePrices.Length)];

                // Обновляем UI
                PhonePriceLabel.Text = $"Телефон за {_selectedPhonePrice:N0} ₽";

                // Обновляем платежи для всех опций
                UpdateInstallmentMonthlyPayments();

                // Сбрасываем выбор
                ResetInstallmentDurationSelection();

                // Показываем модальное окно с анимацией
                InstallmentOfferModal.IsVisible = true;
                InstallmentOfferModal.TranslationY = 800;
                await Task.Delay(50);

                // Сбрасываем на первый шаг
                _installmentCurrentStep = 0;
                ShowInstallmentStep(0);
                UpdateInstallmentStepIndicators();
                InstallmentActionButton.Text = "Продолжить";
                InstallmentActionButton.IsEnabled = false; // Будет активна после выбора срока

                System.Diagnostics.Debug.WriteLine("[DEBUG] Starting InstallmentOfferModal animation");
                await InstallmentOfferModal.TranslateTo(0, 0, 450, Easing.CubicOut);
                System.Diagnostics.Debug.WriteLine("[DEBUG] InstallmentOfferModal animation completed");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] ShowInstallmentOffer: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Обновляет ежемесячные платежи для всех опций
        /// </summary>
        private void UpdateInstallmentMonthlyPayments()
        {
            Monthly6Label.Text = $"≈ {(_selectedPhonePrice / 6):N0} ₽/мес";
            Monthly12Label.Text = $"≈ {(_selectedPhonePrice / 12):N0} ₽/мес";
            Monthly24Label.Text = $"≈ {(_selectedPhonePrice / 24):N0} ₽/мес";
        }

        /// <summary>
        /// Сбрасывает выбор срока рассрочки
        /// </summary>
        private void ResetInstallmentDurationSelection()
        {
            Duration6Months.BackgroundColor = Color.FromArgb("#1B1B1C");
            Duration12Months.BackgroundColor = Color.FromArgb("#1B1B1C");
            Duration24Months.BackgroundColor = Color.FromArgb("#1B1B1C");

            Check6Months.Text = "○";
            Check6Months.TextColor = Color.FromArgb("#3D3D3D");
            Check12Months.Text = "○";
            Check12Months.TextColor = Color.FromArgb("#3D3D3D");
            Check24Months.Text = "○";
            Check24Months.TextColor = Color.FromArgb("#3D3D3D");

            _selectedInstallmentDuration = 0;
        }

        /// <summary>
        /// Обработчик выбора 6 месяцев
        /// </summary>
        private void OnDuration6MonthsTapped(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] OnDuration6MonthsTapped called");
            SelectInstallmentDuration(6);
        }

        /// <summary>
        /// Обработчик выбора 12 месяцев
        /// </summary>
        private void OnDuration12MonthsTapped(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] OnDuration12MonthsTapped called");
            SelectInstallmentDuration(12);
        }

        /// <summary>
        /// Обработчик выбора 24 месяцев
        /// </summary>
        private void OnDuration24MonthsTapped(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[DEBUG] OnDuration24MonthsTapped called");
            SelectInstallmentDuration(24);
        }

        /// <summary>
        /// Выбирает срок рассрочки и обновляет UI
        /// </summary>
        private async void SelectInstallmentDuration(int months)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] SelectInstallmentDuration called with {months} months");

            _selectedInstallmentDuration = months;
            InstallmentActionButton.IsEnabled = true;

            System.Diagnostics.Debug.WriteLine($"[DEBUG] Button enabled: {InstallmentActionButton.IsEnabled}");

            // Сбрасываем все
            ResetInstallmentDurationSelection();

            // Выбираем нужный
            if (months == 6)
            {
                Duration6Months.BackgroundColor = Color.FromArgb("#2B2B2D");
                Check6Months.Text = "●";
                Check6Months.TextColor = Color.FromArgb("#EF3124");
                await Duration6Months.ScaleTo(1.05, 100, Easing.CubicOut);
                await Duration6Months.ScaleTo(1.0, 100, Easing.CubicIn);
            }
            else if (months == 12)
            {
                Duration12Months.BackgroundColor = Color.FromArgb("#2B2B2D");
                Check12Months.Text = "●";
                Check12Months.TextColor = Color.FromArgb("#EF3124");
                await Duration12Months.ScaleTo(1.05, 100, Easing.CubicOut);
                await Duration12Months.ScaleTo(1.0, 100, Easing.CubicIn);
            }
            else if (months == 24)
            {
                Duration24Months.BackgroundColor = Color.FromArgb("#2B2B2D");
                Check24Months.Text = "●";
                Check24Months.TextColor = Color.FromArgb("#EF3124");
                await Duration24Months.ScaleTo(1.05, 100, Easing.CubicOut);
                await Duration24Months.ScaleTo(1.0, 100, Easing.CubicIn);
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки действия в рассрочке
        /// </summary>
        private async void OnInstallmentActionButtonClicked(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] OnInstallmentActionButtonClicked - Step: {_installmentCurrentStep}, Duration: {_selectedInstallmentDuration}");

            if (_installmentCurrentStep == 0)
            {
                // Шаг 1 -> Шаг 2 (Подтверждение)
                if (_selectedInstallmentDuration == 0)
                {
                    System.Diagnostics.Debug.WriteLine("[DEBUG] No duration selected, returning");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("[DEBUG] Moving to step 2 (confirmation)");
                _installmentCurrentStep = 1;
                ShowInstallmentStep(1);
                UpdateInstallmentStepIndicators();
                InstallmentActionButton.Text = "Отправить заявку";

                // Обновляем данные подтверждения
                ConfirmPhoneLabel.Text = "Смартфон";
                ConfirmPriceLabel.Text = $"{_selectedPhonePrice:N0} ₽";
                ConfirmDurationLabel.Text = $"{_selectedInstallmentDuration} {GetMonthsWord(_selectedInstallmentDuration)}";
                ConfirmMonthlyLabel.Text = $"{(_selectedPhonePrice / _selectedInstallmentDuration):N0} ₽";
            }
            else if (_installmentCurrentStep == 1)
            {
                // Шаг 2 -> Шаг 3 (Результат)
                _installmentCurrentStep = 2;
                ShowInstallmentStep(2);
                UpdateInstallmentStepIndicators();
                InstallmentActionButton.Text = "Готово";

                // Генерируем номер заявки
                var applicationNumber = $"#2025-{random.Next(10000000, 99999999)}";
                InstallmentApplicationNumberLabel.Text = applicationNumber;
            }
            else if (_installmentCurrentStep == 2)
            {
                // Шаг 3 -> Закрыть модальное окно
                await HideInstallmentOffer();
            }
        }

        /// <summary>
        /// Возвращает правильную форму слова "месяц"
        /// </summary>
        private string GetMonthsWord(int months)
        {
            if (months == 1 || months == 21)
                return "месяц";
            else if (months >= 2 && months <= 4 || months >= 22 && months <= 24)
                return "месяца";
            else
                return "месяцев";
        }

        /// <summary>
        /// Показывает указанный шаг рассрочки
        /// </summary>
        private void ShowInstallmentStep(int stepIndex)
        {
            InstallmentStep1Content.IsVisible = stepIndex == 0;
            InstallmentStep2Content.IsVisible = stepIndex == 1;
            InstallmentStep3Content.IsVisible = stepIndex == 2;
        }

        /// <summary>
        /// Обновляет индикаторы шагов рассрочки
        /// </summary>
        private void UpdateInstallmentStepIndicators()
        {
            var color1 = _installmentCurrentStep == 0 ? Color.FromArgb("#EF3124") : Color.FromArgb("#2B2B2D");
            var color2 = _installmentCurrentStep == 1 ? Color.FromArgb("#EF3124") : Color.FromArgb("#2B2B2D");
            var color3 = _installmentCurrentStep == 2 ? Color.FromArgb("#EF3124") : Color.FromArgb("#2B2B2D");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                InstallmentStep1Indicator.BackgroundColor = color1;
                InstallmentStep2Indicator.BackgroundColor = color2;
                InstallmentStep3Indicator.BackgroundColor = color3;
            });
        }

        /// <summary>
        /// Скрывает оффер рассрочки
        /// </summary>
        private async Task HideInstallmentOffer()
        {
            await InstallmentOfferModal.TranslateTo(0, 800, 350, Easing.CubicIn);
            InstallmentOfferModal.IsVisible = false;

            // Сбрасываем UI камеры
            HideLoadingUI();
        }

        #endregion

    }
}
