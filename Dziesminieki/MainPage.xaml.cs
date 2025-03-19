using ExcelDataReader;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.ComponentModel.DataAnnotations;

namespace Dziesminieki
{
    public partial class MainPage : ContentPage
    {
        private static MainPage _instance;
        public ObservableCollection<Song> LatvianSongsCollection { get; set; }
        public ObservableCollection<Song> RussianSongsCollection { get; set; }
        public ObservableCollection<Song> FavoriteSongsCollection { get; set; }
        public Dictionary<int, string> LatvianSongs { get; set; }
        public Dictionary<int, string> RussianSongs { get; set; }

        public MainPage()
        {
            InitializeComponent();
            _instance = this;

            //LatvianSongsCollection = new ObservableCollection<Song>();
            //RussianSongsCollection = new ObservableCollection<Song>();
            //FavoriteSongsCollection = new ObservableCollection<Song>();

            LatvianSongsCollection = LoadSongs("LatvianSongs");
            RussianSongsCollection = LoadSongs("RussianSongs");
            FavoriteSongsCollection = new ObservableCollection<Song>();

            LatvianSongs = new Dictionary<int, string>();
            RussianSongs = new Dictionary<int, string>();

            LoadFavoriteSongs();

            this.BindingContext = this;

            double titleFontSize = Preferences.Get("TitleFontSize", 13.0);

            ImportSongsFromExcel("Dziesminieki.Resources.Raw.LatvianSongs.xlsx", LatvianSongs, LatvianSongsCollection, titleFontSize);
            LatvianSongsListView.ItemsSource = LatvianSongsCollection;

            ImportSongsFromExcel("Dziesminieki.Resources.Raw.RussianSongs.xlsx", RussianSongs, RussianSongsCollection, titleFontSize);
            RussianSongsListView.ItemsSource = RussianSongsCollection;

            MessagingCenter.Subscribe<SettingsPage, double>(this, "TitleFontSizeChanged", (sender, fontSize) =>
            {
                foreach (var song in LatvianSongsCollection)
                {
                    song.TitleFontSize = fontSize;
                }

                foreach (var song in RussianSongsCollection)
                {
                    song.TitleFontSize = fontSize;
                }

                LatvianSongsListView.ItemsSource = null;
                LatvianSongsListView.ItemsSource = LatvianSongsCollection;

                RussianSongsListView.ItemsSource = null;
                RussianSongsListView.ItemsSource = RussianSongsCollection;
            });
        }

        public static MainPage Instance => _instance;

        private async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void OnSongTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Song selectedSong)
            {
                await Navigation.PushAsync(new SongDetailPage(selectedSong));
            }
        }

        private async void OnAddSongButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddSongPage());
        }

        private void ImportSongsFromExcel(string filePath, Dictionary<int, string> songDictionary, ObservableCollection<Song> songCollection, double titleFontSize)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = filePath;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Console.WriteLine("Error: Embedded resource not found!");
                    return;
                }

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = reader.AsDataSet();
                    var dataTable = result.Tables[0];

                    for (int i = 0; i < dataTable.Rows.Count; i++)
                    {
                        var number = Convert.ToInt32(dataTable.Rows[i][0]);
                        var title = dataTable.Rows[i][1].ToString();
                        var lyrics = dataTable.Rows[i][2].ToString();

                        var song = new Song
                        {
                            Number = number,
                            Title = title,
                            Lyrics = lyrics,
                            TitleFontSize = titleFontSize
                        };

                        songCollection.Add(song);
                        songDictionary[number] = title;
                    }
                }
            }
        }

        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.NewTextValue))
            {
                if (LatvianSongsListView.IsVisible)
                {
                    LatvianSongsListView.ItemsSource = LatvianSongsCollection;
                }
                else if (RussianSongsListView.IsVisible)
                {
                    RussianSongsListView.ItemsSource = RussianSongsCollection;
                }
                return;
            }

            var searchText = e.NewTextValue;
            var filteredSongs = (LatvianSongsListView.IsVisible ? LatvianSongsCollection : RussianSongsCollection)
                .Where(s => (s.Lyrics != null && s.Lyrics.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                            (s.Number != null && s.Number.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0))
                .DistinctBy(s => s.Number)
                .ToList();

            if (LatvianSongsListView.IsVisible)
            {
                LatvianSongsListView.ItemsSource = filteredSongs;
            }
            else if (RussianSongsListView.IsVisible)
            {
                RussianSongsListView.ItemsSource = filteredSongs;
            }
        }

        private void OnSearchButtonPressed(object sender, EventArgs e)
        {
            var searchBar = sender as SearchBar;
            if (searchBar == null) return;

            var searchText = searchBar.Text;
            if (string.IsNullOrEmpty(searchText))
            {
                if (LatvianSongsListView.IsVisible)
                {
                    LatvianSongsListView.ItemsSource = LatvianSongsCollection;
                }
                else if (RussianSongsListView.IsVisible)
                {
                    RussianSongsListView.ItemsSource = RussianSongsCollection;
                }

                return;
            }

            var filteredSongs = (LatvianSongsListView.IsVisible ? LatvianSongsCollection : RussianSongsCollection)
                .Where(s => (s.Lyrics != null && s.Lyrics.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                            (s.Number != null && s.Number.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0))
                .DistinctBy(s => s.Number)
                .ToList();

            if (LatvianSongsListView.IsVisible)
            {
                LatvianSongsListView.ItemsSource = filteredSongs;
            }
            else if (RussianSongsListView.IsVisible)
            {
                RussianSongsListView.ItemsSource = filteredSongs;
            }
        }

        private void OnLatvianSongsButtonClicked(object sender, EventArgs e)
        {
            LatvianSongsListView.IsVisible = true;
            RussianSongsListView.IsVisible = false;
            FavoriteSongsListView.IsVisible = false;
            SearchBar.Placeholder = "Meklēt dziesmu";
        }

        private void OnRussianSongsButtonClicked(object sender, EventArgs e)
        {
            LatvianSongsListView.IsVisible = false;
            RussianSongsListView.IsVisible = true;
            FavoriteSongsListView.IsVisible = false;
            SearchBar.Placeholder = "Поиск песни";
        }

        private void OnFavoriteSongsButtonClicked(object sender, EventArgs e)
        {
            LatvianSongsListView.IsVisible = false;
            RussianSongsListView.IsVisible = false;
            FavoriteSongsListView.IsVisible = true;
            SearchBar.Placeholder = "Meklē mīļāko dziesmu";
        }

        private void OnFavoriteButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            if (button.CommandParameter is Song song)
            {
                if (FavoriteSongsCollection.Contains(song))
                {
                    FavoriteSongsCollection.Remove(song);
                }
                else
                {
                    FavoriteSongsCollection.Add(song);
                }

                SaveFavoriteSongs();
            }
            else
            {
                Console.WriteLine("Error: CommandParameter is not a valid Song.");
            }
        }

        private void OnRemoveFavoriteButtonClicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;

            if (button.CommandParameter is Song song)
            {
                if (FavoriteSongsCollection.Contains(song))
                {
                    FavoriteSongsCollection.Remove(song);
                    SaveFavoriteSongs();
                }
            }
        }

        public void SaveFavoriteSongs()
        {
            var favoriteSongsJson = JsonSerializer.Serialize(FavoriteSongsCollection);
            Preferences.Set("FavoriteSongs", favoriteSongsJson);
        }

        public void LoadFavoriteSongs()
        {
            var favoriteSongsJson = Preferences.Get("FavoriteSongs", string.Empty);
            if (!string.IsNullOrEmpty(favoriteSongsJson))
            {
                var favoriteSongs = JsonSerializer.Deserialize<ObservableCollection<Song>>(favoriteSongsJson);
                if (favoriteSongs != null)
                {
                    FavoriteSongsCollection = favoriteSongs;
                }
            }
        }

        private ObservableCollection<Song> LoadSongs(string key)
        {
            var songsJson = Preferences.Get(key, string.Empty);
            if (!string.IsNullOrEmpty(songsJson))
            {
                var songs = JsonSerializer.Deserialize<ObservableCollection<Song>>(songsJson);
                if (songs != null)
                {
                    return songs;
                }
            }
            return new ObservableCollection<Song>();
        }

        private async void OnViewCellTapped(object sender, EventArgs e)
        {
            var viewCell = sender as ViewCell;
            if (viewCell != null)
            {
                var originalColor = Color.FromArgb("79303F");
                viewCell.View.BackgroundColor = Color.FromArgb("632431");

                await Task.Delay(200);

                viewCell.View.BackgroundColor = originalColor;
            }
        }
    }

    [Serializable]
    public class Song
    {
        public int? Number { get; set; }
        public string? Title { get; set; }
        public string? Lyrics { get; set; }
        public TextAlignment LyricsAlignment { get; set; }
        public double TitleFontSize { get; set; }

        public string DisplayText => $"{Number} {Title}";
    }
}