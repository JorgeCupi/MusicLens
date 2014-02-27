﻿using MusicLens.ViewModels;
using System;
using System.Windows.Controls;

namespace MusicLens.Controls
{
    public partial class PhotoMosaic : UserControl
    {
        public PhotoMosaic()
        {
            InitializeComponent();
        }

        private void Image_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            PhotoMosaicViewModel m = DataContext as PhotoMosaicViewModel;

            Image i = sender as Image;

            if (m != null && i != null)
            {
                m.RaiseStreamItemTapped(Int32.Parse(i.Name.Replace("Image", "")));
            }
        }
    }
}
