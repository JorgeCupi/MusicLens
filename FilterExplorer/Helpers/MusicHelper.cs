using Microsoft.Xna.Framework.Media;
using MusicLens.Resources;
using System;
using System.Windows;
using System.Windows.Navigation;
namespace MusicLens.Helpers
{
    public class MusicHelpers
    {
        public static void CheckMusic(MediaLibrary library)
        {
            MediaPlayer.IsShuffled = true;
            if (MediaPlayer.Queue.Count == 0)
            {
                if (library.Songs.Count > 0)
                {
                    App.NoMusic = false;
                    MessageBox.Show(AppResources.StreamPage_NoMusic, AppResources.App_Title, new MessageBoxButton());
                    MediaPlayer.Play(library.Songs, new Random().Next(0, library.Songs.Count - 1));
                }
                else {
                    MessageBox.Show(AppResources.StreamPage_NoMusicOnPhone, AppResources.App_Title, new MessageBoxButton());
                    App.txtArtist = AppResources.Music_NoMusic_Artist;
                    App.txtNowPlaying = AppResources.Music_NoMusic_NowPlaying;
                    App.txtSong = AppResources.Music_NoMusic_Song;
                    App.NoMusic = true;
                }
            }
        }
    }
}
