using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Dziesminieki
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<Song> LatvianSongsCollection { get; set; }
        public ObservableCollection<Song> RussianSongsCollection { get; set; }
        public ObservableCollection<Song> FilteredLatvianSongs { get; set; }
        public ObservableCollection<Song> FilteredRussianSongs { get; set; }
        public Dictionary<int, string> LatvianSongs { get; set; }
        public Dictionary<int, string> RussianSongs { get; set; }

        public MainPage()
        {
            InitializeComponent();

            LatvianSongsCollection = new ObservableCollection<Song>();
            RussianSongsCollection = new ObservableCollection<Song>();
            FilteredLatvianSongs = new ObservableCollection<Song>();
            FilteredRussianSongs = new ObservableCollection<Song>();

            LatvianSongs = new Dictionary<int, string>();
            RussianSongs = new Dictionary<int, string>();

            ImportSongsFromExcel("Dziesminieki.Resources.Raw.LatvianSongs.xlsx", LatvianSongs, LatvianSongsCollection);
            LatvianSongsListView.ItemsSource = LatvianSongs.ToList();

            ImportSongsFromExcel("Dziesminieki.Resources.Raw.RussianSongs.xlsx", RussianSongs, RussianSongsCollection);
            RussianSongsListView.ItemsSource = RussianSongs.ToList();

            Console.WriteLine($"Latvian Songs Count: {LatvianSongsCollection.Count}");
            Console.WriteLine($"Russian Songs Count: {RussianSongsCollection.Count}");
        }

        private async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private async void OnSongTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is KeyValuePair<int, string> selectedSong)
            {
                Song song = null;

                if (LatvianSongsListView.IsVisible)
                {
                    song = LatvianSongsCollection.FirstOrDefault(s => s.Number == selectedSong.Key);
                }
                else if (RussianSongsListView.IsVisible)
                {
                    song = RussianSongsCollection.FirstOrDefault(s => s.Number == selectedSong.Key);
                }

                if (song != null)
                {
                    await Navigation.PushAsync(new SongDetailPage(song));
                }
            }
        }

        private void ImportSongsFromExcel(string filePath, Dictionary<int, string> songDictionary, ObservableCollection<Song> songCollection)
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var resourceName = filePath;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    // Handle the case where the resource is not found
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
                            Lyrics = lyrics
                        };

                        songCollection.Add(song);
                        songDictionary[number] = title;
                    }
                }
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
                    FilteredLatvianSongs.Clear();
                    foreach (var song in LatvianSongsCollection)
                    {
                        FilteredLatvianSongs.Add(song);
                    }
                    LatvianSongsListView.ItemsSource = FilteredLatvianSongs;
                }
                else if (RussianSongsListView.IsVisible)
                {
                    FilteredRussianSongs.Clear();
                    foreach (var song in RussianSongsCollection)
                    {
                        FilteredRussianSongs.Add(song);
                    }
                    RussianSongsListView.ItemsSource = FilteredRussianSongs;
                }
                return;
            }

            var filteredSongs = (LatvianSongsListView.IsVisible ? LatvianSongsCollection : RussianSongsCollection)
                .Where(s =>
                    (s.Title?.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (s.Number.HasValue && s.Number.Value.ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                ).ToList();

            if (LatvianSongsListView.IsVisible)
            {
                FilteredLatvianSongs.Clear();
                foreach (var song in filteredSongs)
                {
                    FilteredLatvianSongs.Add(song);
                }
                LatvianSongsListView.ItemsSource = FilteredLatvianSongs;
            }
            else if (RussianSongsListView.IsVisible)
            {
                FilteredRussianSongs.Clear();
                foreach (var song in filteredSongs)
                {
                    FilteredRussianSongs.Add(song);
                }
                RussianSongsListView.ItemsSource = FilteredRussianSongs;
            }
        }

        private void OnLatvianSongsButtonClicked(object sender, EventArgs e)
        {
            LatvianSongsListView.IsVisible = true;
            RussianSongsListView.IsVisible = false;
        }

        private void OnRussianSongsButtonClicked(object sender, EventArgs e)
        {
            LatvianSongsListView.IsVisible = false;
            RussianSongsListView.IsVisible = true;
        }
    }

    public class Song
    {
        public int? Number { get; set; }
        public string? Title { get; set; }
        public string? Lyrics { get; set; }
        public string? Author { get; set; }
        public string? PlaceInBible { get; set; }

        public string DisplayText => $"{Number} {Title}";
    }
}