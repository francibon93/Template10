﻿using System;
using System.Threading.Tasks;
using Template10;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;

namespace Minimal
{
    sealed partial class App : Template10.Common.BootStrapper
    {
        public App()
        {
            InitializeComponent();
            ShowShellBackButton = true;
            CacheMaxDuration = TimeSpan.FromDays(2);
            SplashFactory = (e) => { return new Views.Splash(e); };
        }

        // runs even if restored from state
        public override Task OnInitializeAsync(IActivatedEventArgs args)
        {
            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                Window.Current.Content = new Views.Shell(NavigationService);
            return base.OnInitializeAsync(args);
        }

        // runs unless restored from state
        public override Task OnStartAsync(StartKind startKind, IActivatedEventArgs args)
        {
            Window.Current.Content = new Views.Shell(NavigationService);

            var largs = args as ILaunchActivatedEventArgs;
            if (largs?.TileId != null && largs?.TileId != "App")
            {
                // launched via a secondary tile
                NavigationService.Navigate(typeof(Views.DetailPage), largs.Arguments);
            }
            else
            {
                // launched via other/primary method
                NavigationService.Navigate(typeof(Views.MainPage));
            }
            return Task.FromResult<object>(null);
        }
    }
}
