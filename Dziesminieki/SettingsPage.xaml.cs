using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace Dziesminieki
{
    public partial class SettingsPage : ContentPage
    {
        public ObservableCollection<Song> SongsCollection { get; set; }

        public SettingsPage()
        {
            InitializeComponent();

            TitleSizeSlider.Value = Preferences.Get("TitleFontSize", 15.0);
            LyricsSizeSlider.Value = Preferences.Get("LyricsFontSize", 10.0);
        }

        private async void OnAddSongButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddSongPage());
        }

        private void TitleSizeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            double newSize = e.NewValue;
            Preferences.Set("TitleFontSize", newSize);
            MessagingCenter.Send(this, "TitleFontSizeChanged", newSize);
        }

        private void LyricsSizeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            double newSize = e.NewValue;
            Preferences.Set("LyricsFontSize", newSize);
            MessagingCenter.Send(this, "FontSizeChanged", newSize);
        }

        private void LeftAlignment_Clicked(object sender, EventArgs e)
        {
            Preferences.Set("LyricsAlignment", "Left");
            MessagingCenter.Send(this, "AlignmentChanged", TextAlignment.Start);
        }

        private void CenterAlignment_Clicked(object sender, EventArgs e)
        {
            Preferences.Set("LyricsAlignment", "Center");
            MessagingCenter.Send(this, "AlignmentChanged", TextAlignment.Center);
        }

        private void RightAlignment_Clicked(object sender, EventArgs e)
        {
            Preferences.Set("LyricsAlignment", "Right");
            MessagingCenter.Send(this, "AlignmentChanged", TextAlignment.End);
        }

        private void OnRemoveSongButtonClicked(object sender, EventArgs e)
        {
            if (!int.TryParse(SongNumberEntry.Text, out int number))
            {
                DisplayAlert("Error", "Ievadiet derīgu dziesmas numuru", "OK");
                return;
            }

            if (LanguagePicker.SelectedItem == null)
            {
                DisplayAlert("Error", "Lūdzu izvēlieties valdou", "OK");
                return;
            }

            ObservableCollection<Song> selectedCollection;
            string key;
            if (LanguagePicker.SelectedItem.ToString() == "Latvian")
            {
                selectedCollection = MainPage.Instance.LatvianSongsCollection;
                key = "LatvianSongs";
            }
            else
            {
                selectedCollection = MainPage.Instance.RussianSongsCollection;
                key = "RussianSongs";
            }

            var songToRemove = selectedCollection.FirstOrDefault(s => s.Number == number);
            if (songToRemove != null)
            {
                selectedCollection.Remove(songToRemove);
                SaveSongs(selectedCollection, key);
                DisplayAlert("Success", "Dziesma noņemta", "OK");
            }
            else
            {
                DisplayAlert("Error", "Dziesma nav atrasta", "OK");
            }
        }

        private void SaveSongs(ObservableCollection<Song> songsCollection, string key)
        {
            var songsJson = JsonSerializer.Serialize(songsCollection);
            Preferences.Set(key, songsJson);
        }
    }
}