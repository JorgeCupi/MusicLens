﻿using MusicLens.Controls;
using MusicLens.Models;
using Microsoft.Phone.Controls;
using Nokia.Graphics;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MusicLens
{
    public partial class FilterPage : PhoneApplicationPage
    {
        #region Members

        private bool _busy = false;

        #endregion

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

        #endregion

        public FilterPage()
        {
            InitializeComponent();

            Loaded += FilterPage_Loaded;
        }

        #region Private methods

        private async void FilterPage_Loaded(object sender, RoutedEventArgs e)
        {
            //if (!App.IsTrial)
            //    adDuplexAd.Visibility = System.Windows.Visibility.Collapsed;
            await RenderAsync();
        }

        /// <summary>
        /// Asynchronously renders filter image thumbnails.
        /// </summary>
        private async Task RenderAsync()
        {
            if (!Busy)
            {
                Busy = true;

                int side = 136;

                try
                {
                    Bitmap bitmap = await App.PhotoModel.RenderThumbnailBitmapAsync(side);

                    await RenderThumbnailsAsync(bitmap, side, App.FilterModel.ArtisticFilters, StandardFiltersWrapPanel);
                    await RenderThumbnailsAsync(bitmap, side, App.FilterModel.EnhancementFilters, EnhancementFiltersWrapPanel);
                }
                catch (Exception ex)
                {
                    NavigationService.GoBack();
                }

                Busy = false;
            }
        }

        /// <summary>
        /// For the given bitmap renders filtered thumbnails for each filter in given list and populates
        /// the given wrap panel with the them.
        /// 
        /// For quick rendering, renders 10 thumbnails synchronously and then releases the calling thread.
        /// </summary>
        /// <param name="bitmap">Source bitmap to be filtered</param>
        /// <param name="side">Side length of square thumbnails to be generated</param>
        /// <param name="list">List of filters to be used, one per each thumbnail to be generated</param>
        /// <param name="panel">Wrap panel to be populated with the generated thumbnails</param>
        private async Task RenderThumbnailsAsync(Bitmap bitmap, int side, List<FilterModel> list, WrapPanel panel)
        {
            using (EditingSession session = new EditingSession(bitmap))
            {
                int i = 0;

                foreach (FilterModel filter in list)
                {
                    WriteableBitmap writeableBitmap = new WriteableBitmap(side, side);

                    foreach (IFilter f in filter.Components)
                    {
                        session.AddFilter(f);
                    }

                    Windows.Foundation.IAsyncAction action = session.RenderToBitmapAsync(writeableBitmap.AsBitmap());

                    i++;

                    if (i % 10 == 0)
                    {
                        // async, give control back to UI before proceeding.
                        await action;
                    }
                    else
                    {
                        // synchroneous, we keep the CPU for ourselves.
                        Task task = action.AsTask();
                        task.Wait();
                    }

                    PhotoThumbnail photoThumbnail = new PhotoThumbnail()
                    {
                        Bitmap = writeableBitmap,
                        Text = filter.Name,
                        Width = side,
                        Margin = new Thickness(6)
                    };

                    photoThumbnail.Tap += (object sender, System.Windows.Input.GestureEventArgs e) =>
                    {
                        App.PhotoModel.ApplyFilter(filter);
                        App.PhotoModel.Dirty = true;

                        NavigationService.GoBack();
                    };

                    panel.Children.Add(photoThumbnail);

                    session.UndoAll();
                }
            }
        }

        #endregion
    }
}