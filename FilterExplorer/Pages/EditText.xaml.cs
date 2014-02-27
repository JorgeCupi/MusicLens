using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MusicLens.Resources;

namespace MusicLens.Pages
{
    public partial class EditText : PhoneApplicationPage
    {
        public EditText()
        {
            InitializeComponent();
            colorPicker.ColorChanged += colorPicker_ColorChanged;
        }

        void colorPicker_ColorChanged(object sender, System.Windows.Media.Color color)
        {
            App.colorText = color;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            //if (!App.IsTrial)
            //    adDuplexAd.Visibility = System.Windows.Visibility.Collapsed;

            if (App.txtLyrics.Length == 0)
            {
                txtLyricsSize.Text = "23";
                txtLyrics.Text = AppResources.EditPage_NoLyrics;
            }
            

            txtArtist.Text= App.txtArtist;
            txtSong.Text= App.txtSong;
            txtNowPlaying.Text = App.txtNowPlaying;
            txtLyrics.Text = App.txtLyrics;

            txtArtistSize.Text = App.txtArtistSize.ToString();
            txtNowPlayingSize.Text = App.txtNowPlayingSize.ToString();
            txtSongSize.Text = App.txtSongSize.ToString();
            txtTimeSize.Text = App.txtTimeSize.ToString();
            txtAddressSize.Text = App.txtAddressSize.ToString();
            txtLyricsSize.Text = App.txtLyricsSize.ToString();

        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            App.txtArtist = txtArtist.Text;
            App.txtArtistSize = Double.Parse(txtArtistSize.Text);

            App.txtSong = txtSong.Text;
            App.txtSongSize = Double.Parse(txtSongSize.Text);

            App.txtNowPlaying = txtNowPlaying.Text;
            App.txtNowPlayingSize = Double.Parse(txtNowPlayingSize.Text);

            App.txtAddressSize = Double.Parse(txtAddressSize.Text);
            App.txtTimeSize = Double.Parse(txtTimeSize.Text);

            App.txtLyricsSize = Double.Parse(txtLyricsSize.Text);
            App.txtLyrics = txtLyrics.Text;
        }
    }
}