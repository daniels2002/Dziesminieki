using Microsoft.Maui.Controls;
using System;
using System.IO;

using Microsoft.Maui.Storage;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Fonts;
using System.Reflection;
using PdfSharpCore.Drawing.Layout;

using Microsoft.Maui.Media;
using CommunityToolkit.Maui.Core.Primitives;
using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;

namespace Dziesminieki
{
    public partial class SongDetailPage : ContentPage
    {
        private Song _song;
        private Song _currentSong;
        private string _audioFilePath;
        private ObservableCollection<Song> _currentCollection;
        private double _currentFontSize;

        //private double _lyricsFontSize;

        public SongDetailPage(Song song)
        {
            InitializeComponent();
            _currentSong = song;
            _song = song;

            _currentCollection = MainPage.Instance.LatvianSongsCollection.Contains(song)
                ? MainPage.Instance.LatvianSongsCollection
                : MainPage.Instance.RussianSongsCollection;

            SongLyricsLabel.Text = song.Lyrics;
            SongLyricsLabel.HorizontalTextAlignment = song.LyricsAlignment;

            MessagingCenter.Subscribe<SettingsPage, double>(this, "FontSizeChanged", (sender, fontSize) =>
            {
                SongLyricsLabel.FontSize = fontSize;
                UpdateScrollView();
            });

            MessagingCenter.Subscribe<SettingsPage, TextAlignment>(this, "AlignmentChanged", (sender, alignment) =>
            {
                TextAlignment textAlignment;

                switch (alignment)
                {
                    case TextAlignment.Center:
                        textAlignment = TextAlignment.Center;
                        foreach (var song in MainPage.Instance.LatvianSongsCollection)
                        {
                            SongLyricsLabel.HorizontalTextAlignment = textAlignment;
                        }

                        foreach (var song in MainPage.Instance.RussianSongsCollection)
                        {
                            SongLyricsLabel.HorizontalTextAlignment = textAlignment;
                        }
                        break;

                    case TextAlignment.End:
                        textAlignment = TextAlignment.End;
                        foreach (var song in MainPage.Instance.LatvianSongsCollection)
                        {
                            SongLyricsLabel.HorizontalTextAlignment = textAlignment;
                        }

                        foreach (var song in MainPage.Instance.RussianSongsCollection)
                        {
                            SongLyricsLabel.HorizontalTextAlignment = textAlignment;
                        }
                        break;

                    case TextAlignment.Start:
                    default:
                        textAlignment = TextAlignment.Start;
                        foreach (var song in MainPage.Instance.LatvianSongsCollection)
                        {
                            SongLyricsLabel.HorizontalTextAlignment = textAlignment;
                        }

                        foreach (var song in MainPage.Instance.RussianSongsCollection)
                        {
                            SongLyricsLabel.HorizontalTextAlignment = textAlignment;
                        }
                        break;
                }
            });
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            if (e.Status == GestureStatus.Running)
            {
                double scale = e.Scale;
                double newFontSize = _currentFontSize * scale;

                if (newFontSize < 10)
                    newFontSize = 10;
                if (newFontSize > 30)
                    newFontSize = 30;

                SongLyricsLabel.FontSize = newFontSize;
            }
            else if (e.Status == GestureStatus.Completed)
            {
                _currentFontSize = SongLyricsLabel.FontSize;
            }
        }

        private void UpdateScrollView()
        {
            SongLyricsLabel.InvalidateMeasure();
        }

        private void OnSwipedLeft(object sender, SwipedEventArgs e)
        {
            DisplayNextSong();
        }

        private void OnSwipedRight(object sender, SwipedEventArgs e)
        {
            DisplayPreviousSong();
        }

        private void DisplayNextSong()
        {
            var currentIndex = _currentCollection.IndexOf(_currentSong);
            if (currentIndex < _currentCollection.Count - 1)
            {
                _currentSong = _currentCollection[currentIndex + 1];
                UpdateSongDetails();
            }
        }

        private void DisplayPreviousSong()
        {
            var currentIndex = _currentCollection.IndexOf(_currentSong);
            if (currentIndex > 0)
            {
                _currentSong = _currentCollection[currentIndex - 1];
                UpdateSongDetails();
            }
        }

        private void UpdateSongDetails()
        {
            SongLyricsLabel.Text = _currentSong.Lyrics;
            SongLyricsLabel.HorizontalTextAlignment = _currentSong.LyricsAlignment;
        }

        private async void OnPickAudioButtonClicked(object sender, EventArgs e)
        {
            var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, new[] { "public.audio" } }, // or "public.mp3" for specific file type
                    { DevicePlatform.Android, new[] { "audio/*" } },
                    { DevicePlatform.WinUI, new[] { ".mp3", ".wav", ".wma" } },
                    { DevicePlatform.Tizen, new[] { "*/*" } },
                    { DevicePlatform.macOS, new[] { "public.audio" } }
                });

            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Pick an audio file",
                FileTypes = customFileType
            });

            if (result != null)
            {
                var stream = await result.OpenReadAsync();
                var tempFilePath = Path.Combine(FileSystem.CacheDirectory, result.FileName);

                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }
                AudioPlayer.Source = MediaSource.FromFile(tempFilePath);
            }
        }

        private void OnPlayAudioButtonClicked(object sender, EventArgs e)
        {
            if (AudioPlayer.CurrentState == MediaElementState.Playing)
            {
                AudioPlayer.Pause();
                PlayAudio.Text = "▶";
            }
            else
            {
                AudioPlayer.Play();
                PlayAudio.Text = "▐▐";
            }
        }

        public async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private void OnFavoriteSongsButtonClicked(object sender, EventArgs e)
        {
            if (MainPage.Instance.FavoriteSongsCollection.Contains(_currentSong))
            {
                MainPage.Instance.FavoriteSongsCollection.Remove(_currentSong);
            }
            else
            {
                MainPage.Instance.FavoriteSongsCollection.Add(_currentSong);
            }

            MainPage.Instance.SaveFavoriteSongs();
        }

        private async void OnShareSongButtonClicked(object sender, EventArgs e)
        {
            try
            {
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var titleFont = new XFont("Verdana", 24, XFontStyle.Bold);
                var lyricsFont = new XFont("Verdana", 18, XFontStyle.Regular);

                gfx.DrawString(_currentSong.Title, titleFont, XBrushes.Black,
                    new XRect(0, 0, page.Width, 50),
                    XStringFormats.TopCenter);

                var textFormatter = new XTextFormatter(gfx);
                var textRect = new XRect(20, 60, page.Width - 40, page.Height - 80);

                switch (_currentSong.LyricsAlignment)
                {
                    case TextAlignment.Center:
                        textFormatter.Alignment = XParagraphAlignment.Center;
                        break;

                    case TextAlignment.End:
                        textFormatter.Alignment = XParagraphAlignment.Right;
                        break;

                    case TextAlignment.Start:
                    default:
                        textFormatter.Alignment = XParagraphAlignment.Left;
                        break;
                }

                textFormatter.DrawString(_currentSong.Lyrics, lyricsFont, XBrushes.Black, textRect, XStringFormats.TopLeft);

                using (var stream = new MemoryStream())
                {
                    document.Save(stream, false);
                    stream.Position = 0;

                    var filePath = Path.Combine(FileSystem.CacheDirectory, $"{_currentSong.Title}.pdf");
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var utf8Bytes = stream.ToArray();
                        await fileStream.WriteAsync(utf8Bytes, 0, utf8Bytes.Length);
                    }

                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = _currentSong.Number.ToString(),
                        File = new ShareFile(filePath)
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sharing song: {ex.Message}");
            }
        }

        private async void OnSearchYoutubeButtonClicked(object sender, EventArgs e)
        {
            var songTitle = _currentSong.Title;
            if (!string.IsNullOrEmpty(songTitle))
            {
                var youtubeSearchUrl = $"https://www.youtube.com/results?search_query={Uri.EscapeDataString(songTitle)}";
                await Launcher.OpenAsync(new Uri(youtubeSearchUrl));
            }
        }
    }

    public class FontResolver : IFontResolver
    {
        public static readonly FontResolver Instance = new FontResolver();

        public string DefaultFontName => "Verdana";

        public byte[] GetFont(string faceName)
        {
            var assembly = typeof(FontResolver).GetTypeInfo().Assembly;
            var resource = faceName switch
            {
                "Verdana#Regular" => "Dziesminieki.Resources.Fonts.Verdana.ttf",
                "Verdana#Bold" => "Dziesminieki.Resources.Fonts.Verdana-Bold.ttf",
                _ => throw new NotImplementedException()
            };

            using (var stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream == null)
                    throw new FileNotFoundException("Font not found.", resource);

                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (familyName.Equals("Verdana", StringComparison.OrdinalIgnoreCase))
            {
                if (isBold)
                    return new FontResolverInfo("Verdana#Bold");
                return new FontResolverInfo("Verdana#Regular");
            }

            throw new NotImplementedException();
        }
    }
}