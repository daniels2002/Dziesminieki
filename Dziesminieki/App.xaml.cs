using PdfSharpCore.Fonts;

namespace Dziesminieki
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            GlobalFontSettings.FontResolver = FontResolver.Instance;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}