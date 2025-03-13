using Microsoft.Maui.Controls;
using System;
using System.IO;
using Plugin.Maui.Audio;
using Microsoft.Maui.Storage;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Fonts;
using System.Reflection;
using PdfSharpCore.Drawing.Layout;

namespace Dziesminieki
{
    public partial class SongDetailPage : ContentPage
    {
        private Song _song;
        private Song _currentSong;
        private double _lyricsFontSize;

        public SongDetailPage(Song song)
        {
            InitializeComponent();
            _currentSong = song;
            _song = song;

            SongLyricsLabel.Text = song.Lyrics;
            SongLyricsLabel.HorizontalTextAlignment = song.LyricsAlignment;
            //song.LyricsAlignment = alignment;
            // Subscribe to the font size change message
            MessagingCenter.Subscribe<SettingsPage, double>(this, "FontSizeChanged", (sender, fontSize) =>
            {
                SongLyricsLabel.FontSize = fontSize;
            });

            MessagingCenter.Subscribe<SettingsPage, TextAlignment>(this, "AlignmentChanged", (sender, alignment) =>
            {
                SongLyricsLabel.HorizontalTextAlignment = alignment;
            });
        }

        public async void OnSettingsButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SettingsPage());
        }

        private void OnFavoriteSongsButtonClicked(object sender, EventArgs e)
        {
            // Add or remove the current song from the favorites
            if (MainPage.Instance.FavoriteSongsCollection.Contains(_currentSong))
            {
                MainPage.Instance.FavoriteSongsCollection.Remove(_currentSong);
            }
            else
            {
                MainPage.Instance.FavoriteSongsCollection.Add(_currentSong);
            }

            // Save the updated favorite songs collection
            MainPage.Instance.SaveFavoriteSongs();
        }

        private async void OnShareSongButtonClicked(object sender, EventArgs e)
        {
            try
            {
                // Create a PDF document
                var document = new PdfDocument();
                var page = document.AddPage();
                var gfx = XGraphics.FromPdfPage(page);
                var titleFont = new XFont("Verdana", 24, XFontStyle.Bold);
                var lyricsFont = new XFont("Verdana", 18, XFontStyle.Regular);

                // Draw the song title
                gfx.DrawString(_currentSong.Title, titleFont, XBrushes.Black,
                    new XRect(0, 0, page.Width, 50),
                    XStringFormats.TopCenter);

                // Draw the song lyrics with word wrapping
                var textFormatter = new XTextFormatter(gfx);
                var textRect = new XRect(20, 60, page.Width - 40, page.Height - 80);

                // Set alignment based on LyricsAlignment
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

                // Save the document to a stream
                using (var stream = new MemoryStream())
                {
                    document.Save(stream, false);
                    stream.Position = 0;

                    // Save the stream to a file with UTF-8 encoding
                    var filePath = Path.Combine(FileSystem.CacheDirectory, $"{_currentSong.Title}.pdf");
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        var utf8Bytes = stream.ToArray();
                        await fileStream.WriteAsync(utf8Bytes, 0, utf8Bytes.Length);
                    }

                    // Share the file
                    await Share.RequestAsync(new ShareFileRequest
                    {
                        Title = _currentSong.Number.ToString(),
                        File = new ShareFile(filePath)
                    });
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error, show a message to the user)
                Console.WriteLine($"Error sharing song: {ex.Message}");
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