using Microsoft.Maui.Controls;

namespace Dziesminieki
{
    public partial class SongDetailPage : ContentPage
    {
        public SongDetailPage(Song song)
        {
            InitializeComponent();

            //SongTitleLabel.Text = song.Title;
            SongLyricsLabel.Text = song.Lyrics;
        }
    }
}