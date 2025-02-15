﻿using Minimal.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Minimal.Views
{
    public sealed partial class DetailPage : Page
    {
        public DetailPage()
        {
            InitializeComponent();
            NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;
        }

        // strongly-typed view models enable x:bind
        public DetailPageViewModel ViewModel { get { return DataContext as DetailPageViewModel; } }
    }
}
