using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace MusicLens.Pages
{
    public partial class About : PhoneApplicationPage
    {
        public About()
        {
            InitializeComponent();

            this.Loaded += About_Loaded;
        }

        void About_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!App.IsTrial)
            //    adDuplexAd.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
}