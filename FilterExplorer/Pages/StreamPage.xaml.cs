using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using MusicLens.Models;
using MusicLens.Resources;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows;
using System.Windows.Threading;
namespace MusicLens
{
    public partial class StreamPage : PhoneApplicationPage
    {
        private MediaLibrary library;
        private PhotoChooserTask _photoChooserTask = new PhotoChooserTask();
        private CameraCaptureTask _cameraCaptureTask = new CameraCaptureTask();

        public StreamPage()
        {
            InitializeComponent();

            btnTakePic.Tap += btnTakePic_Tap;
            btnPickFromPictures.Tap += btnPickFromPictures_Tap;
            btnRateMe.Tap += btnRateMe_Tap;
            btnAbout.Tap += btnAbout_Tap;
            _photoChooserTask.Completed += Task_Completed;
            _cameraCaptureTask.Completed += Task_Completed;

            this.Loaded += StreamPage_Loaded;
            library = new MediaLibrary();

            //if (!App.IsTrial)
            //    adDuplexAd.Visibility = System.Windows.Visibility.Collapsed;
        }

        void btnAbout_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/AboutPage.xaml",UriKind.Relative));
        }

        private void StreamPage_Loaded(object sender, RoutedEventArgs e)
        {
            FrameworkDispatcher.Update();
            Helpers.MusicHelpers.CheckMusic(library);
        }

        private void btnPickFromPictures_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            _photoChooserTask.Show();
        }

        private void btnTakePic_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            _cameraCaptureTask.Show();
        }

        void btnRateMe_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            MarketplaceReviewTask task = new MarketplaceReviewTask();
            task.Show();
        }

        private void Task_Completed(object sender, PhotoResult e)
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

                    Dispatcher.BeginInvoke(() => { NavigationService.Navigate(new Uri("/Pages/PhotoPage.xaml", UriKind.Relative)); });
                }
                else
                {
                    MessageBox.Show(AppResources.App_MessageBox_UnsupportedImage_Message,
                        AppResources.App_MessageBox_UnsupportedImage_Caption, MessageBoxButton.OK);
                }
            }
        }
    }
}