namespace BlackJackCamera
{
    /// <summary>
    /// Главный класс приложения
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Инициализирует новый экземпляр приложения
        /// </summary>
        /// <param name="serviceProvider">Провайдер сервисов для DI</param>
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            MainPage = new AppShell
            {
                BindingContext = serviceProvider
            };
        }

        /// <summary>
        /// Создает главное окно приложения
        /// </summary>
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(MainPage!);
        }
    }
}