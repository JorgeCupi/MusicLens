using Microsoft.Phone.Controls;
using Microsoft.Phone.Maps.Services;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using MusicLens.Models;
using MusicLens.Resources;
using System;
using System.Device.Location;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using System.Net.Http;
using Newtonsoft.Json;
namespace MusicLens
{
    public partial class PhotoPage : PhoneApplicationPage
    {
        #region Members

        private MediaLibrary library;

        private PhotoChooserTask _photoChooserTask = new PhotoChooserTask();
        private CameraCaptureTask _cameraCaptureTask = new CameraCaptureTask();

        private bool _busy = false;
        private bool savedPicture;
        private bool cameBack;

        private GeoCoordinateWatcher watcher;

        private PhotoChooserTask pictureTask;
        private ShareMediaTask mediaTask;

        private ApplicationBarIconButton btnAdd = null;
        private ApplicationBarIconButton btnMusic = null;
        private ApplicationBarIconButton btnShare = null;
        private ApplicationBarMenuItem menuText = null;
        private ApplicationBarMenuItem menuClearFilters = null;
        private ApplicationBarMenuItem menuShowTime = null;
        private ApplicationBarMenuItem menuShowAddress = null;
        private ApplicationBarMenuItem menuShowLyrics = null;

        private const double _maxWidth = 2048;
        private const double _maxHeight = 2048;

        private double _width = 0;
        private double _height = 0;

        private double _scale = 1.0;

        private bool _highQuality = false;
        private bool _loaded = false;
        private bool cameFromLens = false;


        private string lyricsNMusicKey = "24e2853f3d450613f86e8e74698a10";
        #endregion Members

        #region Properties

        private bool Busy
        {
            get
            {
                return _busy;
            }

            set
            {
                if (_busy != value)
                {
                    _busy = value;
                }
            }
        }

        private bool HighQuality
        {
            get
            {
                return _highQuality;
            }

            set
            {
                if (_highQuality != value)
                {
                    _highQuality = value;

                    if (_highQuality)
                    {
                        pgrAddress.Visibility = Visibility.Visible;
                        RenderAsync();
                        pgrAddress.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        #endregion Properties

        public PhotoPage()
        {
            InitializeComponent();
            _cameraCaptureTask.Completed += _cameraCaptureTask_Completed;
            this.OrientationChanged += PhotoPage_OrientationChanged;
            library = new MediaLibrary();
            Loaded += PhotoPage_Loaded;
            LoadAppBar();

            App.colorText = Colors.White;
            App.txtArtistSize = 55;
            App.txtSongSize = 55;
            App.txtNowPlayingSize = 32;
            App.txtTimeSize = 32;
            App.txtAddressSize = 32;
            App.txtLyricsSize = 23;

            cameBack = false;
            savedPicture = false;

            try
            {
                App.txtNowPlaying = "#NowPlaying";
                App.txtArtist = MediaPlayer.Queue.ActiveSong.Artist.Name;
                App.txtSong = MediaPlayer.Queue.ActiveSong.Name;
                App.txtLyrics = String.Empty;
            }
            catch (Exception)
            {}

            mediaTask = new ShareMediaTask();
            pictureTask = new PhotoChooserTask();
            pictureTask.Completed += pictureTask_Completed;

            watcher = new GeoCoordinateWatcher();
            watcher.MovementThreshold = 500;
            watcher.PositionChanged += watcher_PositionChanged;
            watcher.StatusChanged += watcher_StatusChanged;
        }

        void PhotoPage_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            double height = Application.Current.RootVisual.RenderSize.Height;
            double width = Application.Current.RootVisual.RenderSize.Width;
            if (e.Orientation == PageOrientation.PortraitUp)
                imgLogo.Margin = new Thickness(width - imgLogo.Width, height - imgLogo.Height - 150, 0, 0);
            else
            {
                if (e.Orientation == PageOrientation.LandscapeLeft || e.Orientation == PageOrientation.LandscapeRight)
                    imgLogo.Margin = new Thickness(height - imgLogo.Width - 150, width - imgLogo.Height, 0, 0);
            }        
        }

        private void PhotoPage_Loaded(object sender, RoutedEventArgs e)
        {
            _loaded = true;
            if(!cameFromLens)
                RenderAsync();
            if (!App.NoMusic)
                LoadSongInfo();
            else NoMusic();
        }

        private void LoadSongInfo()
        {
            if (!cameBack)
            {
                App.txtNowPlaying = "#NowPlaying";
                App.txtArtist = MediaPlayer.Queue.ActiveSong.Artist.Name;
                App.txtSong = MediaPlayer.Queue.ActiveSong.Name;
                App.txtLyrics = String.Empty;
                cameBack = false;
            }

            txtArtist.Text = App.txtArtist;
            txtSong.Text = App.txtSong;
            txtNowPlaying.Text = App.txtNowPlaying;
            txtLyrics.Text = App.txtLyrics;

            SolidColorBrush brush = new SolidColorBrush(App.colorText);
            txtArtist.Foreground = brush;
            txtSong.Foreground = brush;
            txtNowPlaying.Foreground = brush;
            txtAddress.Foreground = brush;
            txtTime.Foreground = brush;
            txtLyrics.Foreground = brush;

            txtArtist.FontSize = App.txtArtistSize;
            txtSong.FontSize = App.txtSongSize;
            txtNowPlaying.FontSize = App.txtNowPlayingSize;
            txtAddress.FontSize = App.txtAddressSize;
            txtTime.FontSize = App.txtTimeSize;
            txtLyrics.FontSize = App.txtLyricsSize;
        }

        #region AppBar methods

        private void LoadAppBar()
        {
            btnAdd = new ApplicationBarIconButton();
            btnAdd.Text = AppResources.PhotoPage_Button_AddFilter;
            btnAdd.IconUri = new Uri("/Assets/Icons/add.png", UriKind.Relative);
            btnAdd.Click += AddButton_Click;
            ApplicationBar.Buttons.Add(btnAdd);

            btnMusic = new ApplicationBarIconButton();
            btnMusic.Text = AppResources.PhotoPage_Button_StartMusic;
            btnMusic.IconUri = new Uri("/Assets/Icons/music.png", UriKind.Relative);
            btnMusic.Click += MusicButton_Click;
            ApplicationBar.Buttons.Add(btnMusic);

            btnShare = new ApplicationBarIconButton();
            btnShare.Text = AppResources.PhotoPage_Button_Share;
            btnShare.IconUri = new Uri("/Assets/Icons/share.png", UriKind.Relative);
            btnShare.Click += ShareButton_Click;
            ApplicationBar.Buttons.Add(btnShare);

            menuText = new ApplicationBarMenuItem();
            menuText.Text = AppResources.PhotoPage_Menu_Text;
            menuText.Click += menuText_Click;
            ApplicationBar.MenuItems.Add(menuText);

            menuClearFilters = new ApplicationBarMenuItem();
            menuClearFilters.Text = AppResources.PhotoPage_Menu_Clear_Filters;
            menuClearFilters.Click += menuClearFilters_Click;
            ApplicationBar.MenuItems.Add(menuClearFilters);

            menuShowTime = new ApplicationBarMenuItem();
            menuShowTime.Text = AppResources.PhotoPage_Menu_Show_Time;
            menuShowTime.Click += menuShowTime_Click;
            ApplicationBar.MenuItems.Add(menuShowTime);

            menuShowAddress = new ApplicationBarMenuItem();
            menuShowAddress.Text = AppResources.PhotoPage_Menu_Show_Address;
            menuShowAddress.Click += menuShowAddress_Click;
            ApplicationBar.MenuItems.Add(menuShowAddress);

            menuShowLyrics = new ApplicationBarMenuItem();
            menuShowLyrics.Text = AppResources.PhotoPage_Menu_Show_Lyrics;
            menuShowLyrics.Click += menuShowLyrics_Click;
            ApplicationBar.MenuItems.Add(menuShowLyrics);
        }

        async void menuShowLyrics_Click(object sender, EventArgs e)
        {
            //if (!App.IsTrial)
            //{
                if (menuShowLyrics.Text == AppResources.PhotoPage_Menu_Show_Lyrics)
                {
                    txtLyrics.Visibility = System.Windows.Visibility.Visible;
                    menuShowLyrics.Text = AppResources.PhotoPage_Menu_Show_Lyrics_No;

                    string lyric = String.Empty;
                    string unformatedUri = "http://api.lyricsnmusic.com/songs?api_key={0}&artist={1}&track={2}";
                    string uri = String.Format(unformatedUri, lyricsNMusicKey, txtArtist.Text, txtSong.Text);

                    HttpClient client = new HttpClient();
                    try
                    {
                        string result = await client.GetStringAsync(uri);
                        dynamic lyrics = JsonConvert.DeserializeObject(result);
                        foreach (var item in lyrics)
                        {
                            lyric = item.snippet.ToString();
                            if (lyric != null)
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        lyric = AppResources.PhotoPage_NoInternet;
                    }

                    if(lyric.Length==0)
                        lyric = AppResources.PhotoPage_NoLyrics;

                    App.txtLyrics = lyric;
                    txtLyrics.Text = App.txtLyrics;
                }
                else 
                {
                    txtLyrics.Visibility = System.Windows.Visibility.Collapsed;
                    menuShowLyrics.Text = AppResources.PhotoPage_Menu_Show_Lyrics;
                }
            //}
            //else AskToBuy();
        }

        private void menuShowAddress_Click(object sender, EventArgs e)
        {
            //if (!App.IsTrial)
            //{
                if (menuShowAddress.Text == AppResources.PhotoPage_Menu_Show_Address)
                {
                    menuShowAddress.Text = AppResources.PhotoPage_Menu_Show_Address_No;
                    LayoutRoot.Opacity = 0.5;
                    pgrAddress.Visibility = System.Windows.Visibility.Visible;
                    watcher = new GeoCoordinateWatcher();
                    watcher.StatusChanged += watcher_StatusChanged;
                    watcher.PositionChanged += watcher_PositionChanged;
                    watcher.Start();
                }
                else
                {
                    txtAddress.Visibility = System.Windows.Visibility.Collapsed;
                    menuShowAddress.Text = AppResources.PhotoPage_Menu_Show_Address;

                    LayoutRoot.Opacity = 1;
                    pgrAddress.Visibility = System.Windows.Visibility.Collapsed;
                }
            //}
            //else
            //    AskToBuy();
        }

        private void AskToBuy()
        {
            MessageBoxResult result = MessageBox.Show(AppResources.App_IsTrial, AppResources.App_Title, MessageBoxButton.OKCancel);
            if (result == MessageBoxResult.OK)
            {
                MarketplaceDetailTask task = new MarketplaceDetailTask();
                task.Show();
            }
        }

        private void menuShowTime_Click(object sender, EventArgs e)
        {
            txtTime.Text = DateTime.Now.TimeOfDay.Hours + ":" + DateTime.Now.TimeOfDay.Minutes;
            if (menuShowTime.Text == AppResources.PhotoPage_Menu_Show_Time)
            {
                txtTime.Visibility = System.Windows.Visibility.Visible;
                menuShowTime.Text = AppResources.PhotoPage_Menu_Show_Time_No;
            }
            else
            {
                txtTime.Visibility = System.Windows.Visibility.Collapsed;
                menuShowTime.Text = AppResources.PhotoPage_Menu_Show_Time;
            }
        }

        private void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    NoGPSData(AppResources.PhotoPage_Address_GPSDisabled);
                    break;
                case GeoPositionStatus.NoData:
                    NoGPSData(AppResources.PhotoPage_Address_Error);
                    break;
                default:
                    break;
            }
        }

        private void NoGPSData(string p)
        {
            watcher.Dispose();
            pgrAddress.Visibility = System.Windows.Visibility.Collapsed;
            LayoutRoot.Opacity = 1;
            txtAddress.Text = p;
            txtAddress.Visibility = System.Windows.Visibility.Visible;
        }

        private void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            ReverseGeocodeQuery query = new ReverseGeocodeQuery();
            query.GeoCoordinate = new GeoCoordinate(e.Position.Location.Latitude, e.Position.Location.Longitude);
            query.QueryCompleted += (s, ee) =>
            {
                txtAddress.Visibility = System.Windows.Visibility.Visible;
                LayoutRoot.Opacity = 1;
                pgrAddress.Visibility = System.Windows.Visibility.Collapsed;
                if (ee.Error == null && ee.Result.Count > 0)
                {
                    txtAddress.Text = AppResources.PhotoPage_Address + " " +
                        ee.Result.First().Information.Address.District + " - " +
                        ee.Result.First().Information.Address.City;
                }
                else
                {
                    txtAddress.Text = AppResources.PhotoPage_Address_Error;
                }
            };
            query.QueryAsync();
            watcher.Dispose();
        }

        private async void menuClearFilters_Click(object sender, EventArgs e)
        {
            while (App.PhotoModel.AppliedFilters.Count > 0)
            {
                App.PhotoModel.UndoFilter();
            }
            App.PhotoModel.Dirty = !App.PhotoModel.CanUndoFilter && App.PhotoModel.Captured || App.PhotoModel.CanUndoFilter;

            Setup();
            pgrAddress.Visibility = Visibility.Visible;
            await RenderAsync();
            pgrAddress.Visibility = Visibility.Collapsed;
        }

        private void menuText_Click(object sender, EventArgs e)
        {
            //if (!App.IsTrial)
                NavigationService.Navigate(new Uri("/Pages/EditText.xaml", UriKind.Relative));
            //else AskToBuy();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/FilterPage.xaml", UriKind.Relative));
        }

        private void ShareButton_Click(object sender, EventArgs e)
        {
            if (!savedPicture)
                SavePicture();
            mediaTask.Show();
        }

        private void SavePicture()
        {
            MemoryStream stream = new MemoryStream();
            WriteableBitmap bitmap = new WriteableBitmap(LayoutRoot, null);
            bitmap.SaveJpeg(stream, bitmap.PixelWidth, bitmap.PixelHeight, 0, 100);
            stream.Seek(0, SeekOrigin.Begin);
            MediaLibrary mediaLibrary = new MediaLibrary();
            mediaLibrary.SavePicture("Picture.jpg", stream);
            stream.Close();

            string fullPath = MediaLibraryExtensions.GetPath(mediaLibrary.SavedPictures.Last());
            mediaTask.FilePath = fullPath;
            MessageBox.Show(AppResources.PhotoPage_Image_Saved, AppResources.App_Title, new MessageBoxButton());
            savedPicture = true;
        }

        private void pictureTask_Completed(object sender, PhotoResult e)
        {
            mediaTask.FilePath = e.OriginalFileName;
            mediaTask.Show();
        }

        private void MusicButton_Click(object sender, EventArgs e)
        {
            
            if (!App.NoMusic)
            {
                MediaPlayer.MoveNext();
                cameBack = false;
                LoadSongInfo();

                if (menuShowLyrics.Text == AppResources.PhotoPage_Menu_Show_Lyrics_No)
                {
                    txtLyrics.Visibility = System.Windows.Visibility.Collapsed;
                    menuShowLyrics.Text = AppResources.PhotoPage_Menu_Show_Lyrics;
                }
            }
            else NoMusic();
        }

        private void NoMusic()
        {
            txtArtist.Text = AppResources.Music_NoMusic_Artist;
            txtNowPlaying.Text = AppResources.Music_NoMusic_NowPlaying;
            txtSong.Text = AppResources.Music_NoMusic_Song;
        }

        #endregion AppBar methods

        #region More crap

        private void Setup()
        {
            try
            {
                double scale;

                if (App.PhotoModel.Width > App.PhotoModel.Height)
                {
                    scale = _maxWidth / App.PhotoModel.Width;
                }
                else
                {
                    scale = _maxHeight / App.PhotoModel.Height;
                }

                _width = App.PhotoModel.Width * scale;
                _height = App.PhotoModel.Height * scale;

                HighQuality = false;
            }
            catch (Exception ex)
            {
            }
        }

        private async Task RenderAsync()
        {
            if (!Busy)
            {
                Busy = true;

                bool hq;

                do
                {
                    hq = _highQuality;

                    double w = hq ? _width : _width * _scale;
                    double h = hq ? _height : _height * _scale;

                    WriteableBitmap writeableBitmap = new WriteableBitmap((int)w, (int)h);

                    await App.PhotoModel.RenderBitmapAsync(writeableBitmap);

                    Image.Source = writeableBitmap;
                    double height = Application.Current.RootVisual.RenderSize.Height;
                        double width = Application.Current.RootVisual.RenderSize.Width;
                    imgLogo.Margin = new Thickness(width - imgLogo.Width, height - imgLogo.Height, 0, 0);

                }
                while (hq != _highQuality);

                Busy = false;
            }
        }

        #endregion More crap

        #region Protected methods

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            FrameworkDispatcher.Update();
            Helpers.MusicHelpers.CheckMusic(library);

            base.OnNavigatedTo(e);

            if (App.PhotoModel != null)
            {
                if (!Busy)
                {
                    Setup();
                }
            }
            else 
            {
                if (e.Uri.ToString().Contains("ViewfinderLaunch"))
                {
                    if (e.NavigationMode == NavigationMode.Back)
                        Application.Current.Terminate();
                    else
                    {
                        _cameraCaptureTask.Show();
                        cameFromLens = true;
                    }
                        
                }
                if (e.Uri.ToString().Contains("EditPhotoContent"))
                {
                    if (e.NavigationMode == NavigationMode.Back)
                        Application.Current.Terminate();
                    else
                        SetPhotoFromEditedPage(NavigationContext.QueryString["FileId"]);
                }
            }

            txtArtist.SetValue(Canvas.LeftProperty, 0.0);
            txtSong.SetValue(Canvas.LeftProperty, 0.0);
            txtNowPlaying.SetValue(Canvas.LeftProperty, 0.0);

            txtSong.SetValue(Canvas.TopProperty, 0.0);
            txtArtist.SetValue(Canvas.TopProperty, 50.0);
            txtNowPlaying.SetValue(Canvas.TopProperty, 100.0);

            if (e.NavigationMode == NavigationMode.Back)
            {
                cameBack = true;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (e.NavigationMode == NavigationMode.Back && e.IsCancelable)
            {
                if (Busy)
                {
                    e.Cancel = true;
                }
            }
        }

        async void _cameraCaptureTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                if (Helpers.FileHelpers.IsValidPicture(e.OriginalFileName))
                {
                    if (App.PhotoModel != null)
                    {
                        App.PhotoModel.Dispose();
                        App.PhotoModel = null;

                        GC.Collect();
                    }

                    using (MemoryStream stream = new MemoryStream())
                    {
                        e.ChosenPhoto.CopyTo(stream);

                        App.PhotoModel = new PhotoModel() { Buffer = stream.GetWindowsRuntimeBuffer() };
                        App.PhotoModel.Captured = (sender == _cameraCaptureTask);
                        App.PhotoModel.Dirty = App.PhotoModel.Captured;
                    }

                    if (_loaded)
                    {
                        Setup();
                        pgrAddress.Visibility = System.Windows.Visibility.Visible;
                        await RenderAsync();
                        pgrAddress.Visibility = System.Windows.Visibility.Collapsed;
                    }
                }
                else
                {
                    MessageBox.Show(AppResources.App_MessageBox_UnsupportedImage_Message,
                        AppResources.App_MessageBox_UnsupportedImage_Caption, MessageBoxButton.OK);
                }
            }
        }


        private async void SetPhotoFromEditedPage(string FiledID)
        {
            MediaLibrary library = new MediaLibrary();
            Picture photoFromLibrary = library.GetPictureFromToken(FiledID);

            using (MemoryStream stream = new MemoryStream())
            {
                photoFromLibrary.GetImage().CopyTo(stream);

                App.PhotoModel = new PhotoModel() { Buffer = stream.GetWindowsRuntimeBuffer() };
                App.PhotoModel.Captured = true;
                App.PhotoModel.Dirty = true;
                
                Setup();
                pgrAddress.Visibility = System.Windows.Visibility.Visible;
                await RenderAsync();
                pgrAddress.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
        #endregion Protected methods
    }
}