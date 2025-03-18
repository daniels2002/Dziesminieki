using System.Collections.ObjectModel;
using System.Text.Json;
using System.Linq;

namespace Dziesminieki;

public partial class AddSongPage : ContentPage
{
    public AddSongPage()
    {
        InitializeComponent();
    }

    private async void OnAddSongButtonClicked(object sender, EventArgs e)
    {
        if (!int.TryParse(SongNumberEntry.Text, out int number))
        {
            await DisplayAlert("Error", "Lūdzu ievadied derīgu dziesmas numuru", "OK");
            return;
        }

        if (string.IsNullOrEmpty(SongTitleEntry.Text))
        {
            await DisplayAlert("Error", "Lūdzu ievadied dziesmas nosaukumu", "OK");
            return;
        }

        if (string.IsNullOrEmpty(SongLyricsEditor.Text))
        {
            await DisplayAlert("Error", "Lūdzu ievadied dziesmas tekstu", "OK");
            return;
        }

        if (LanguagePicker.SelectedItem == null)
        {
            await DisplayAlert("Error", "Lūdzu izvēlaties valdou", "OK");
            return;
        }

        double titleFontSize = Preferences.Get("TitleFontSize", 15.0);

        ObservableCollection<Song> selectedCollection;
        if (LanguagePicker.SelectedItem.ToString() == "Latvian")
        {
            selectedCollection = MainPage.Instance.LatvianSongsCollection;
        }
        else
        {
            selectedCollection = MainPage.Instance.RussianSongsCollection;
        }

        // Check if the song number already exists
        if (selectedCollection.Any(s => s.Number == number))
        {
            int maxNumber = selectedCollection.Max(s => s.Number) ?? 0;
            await DisplayAlert("Error", $"Dziesma ar šo numuru jau eksistē. Lūdzu izvēlieties numuru, kas ir lielāks par {maxNumber}.", "OK");
            return;
        }

        var song = new Song
        {
            Number = number,
            Title = SongTitleEntry.Text,
            Lyrics = SongLyricsEditor.Text,
            TitleFontSize = titleFontSize
        };

        if (LanguagePicker.SelectedItem.ToString() == "Latvian")
        {
            MainPage.Instance.LatvianSongsCollection.Add(song);
            MainPage.Instance.LatvianSongs[number] = song.Title;
            SaveSongs(MainPage.Instance.LatvianSongsCollection, "LatvianSongs");
        }
        else if (LanguagePicker.SelectedItem.ToString() == "Russian")
        {
            MainPage.Instance.RussianSongsCollection.Add(song);
            MainPage.Instance.RussianSongs[number] = song.Title;
            SaveSongs(MainPage.Instance.RussianSongsCollection, "RussianSongs");
        }

        await Navigation.PopAsync();
    }

    private void SaveSongs(ObservableCollection<Song> songsCollection, string key)
    {
        var songsJson = JsonSerializer.Serialize(songsCollection);
        Preferences.Set(key, songsJson);
    }
}