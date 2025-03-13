using Microsoft.Maui.Controls;

namespace Dziesminieki
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void TitleSizeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            double newSize = (int)e.NewValue;
            Preferences.Set("TitleFontSize", newSize);
            MessagingCenter.Send(this, "FontSizeChanged", newSize);
        }

        private void LyricsSizeSlider1_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            double newSize = (int)e.NewValue;
            Preferences.Set("LyricsFontSize", newSize);
            MessagingCenter.Send(this, "FontSizeChanged", newSize);
        }

        private void LeftAlignment_Clicked(object sender, EventArgs e)
        {
            Preferences.Set("LyricsAlignment", "Left");
            MessagingCenter.Send(this, "AlignmentChanged", "Left");
        }

        private void CenterAlignment_Clicked(object sender, EventArgs e)
        {
            Preferences.Set("LyricsAlignment", "Center");
            MessagingCenter.Send(this, "AlignmentChanged", "Center");
        }

        private void RightAlignment_Clicked(object sender, EventArgs e)
        {
            Preferences.Set("LyricsAlignment", "Right");
            MessagingCenter.Send(this, "AlignmentChanged", "Right");
        }
    }
}