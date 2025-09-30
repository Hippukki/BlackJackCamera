namespace BlackJackCamera
{
    /// <summary>
    /// Оболочка приложения (Shell) для навигации
    /// </summary>
    public partial class AppShell : Shell
    {
        /// <summary>
        /// Инициализирует новый экземпляр AppShell
        /// </summary>
        public AppShell()
        {
            InitializeComponent();

            // Регистрация маршрутов для навигации
            Routing.RegisterRoute(nameof(CameraPage), typeof(CameraPage));
        }
    }
}
