﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Template10.Common;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Template10.Services.NavigationService
{
    public class NavigationService
    {
        private const string EmptyNavigation = "1,0";

        public FrameFacade FrameFacade { get; private set; }
        public Frame Frame { get { return FrameFacade.Frame; } }
        object LastNavigationParameter { get; set; }
        string LastNavigationType { get; set; }

        public NavigationService(Frame frame)
        {
            FrameFacade = new FrameFacade(frame);
            FrameFacade.Navigating += async (s, e) =>
            {
                if (e.Suspending)
                    return;

                // allow the viewmodel to cancel navigation
                e.Cancel = !NavigatingFrom(false);
                if (!e.Cancel)
                {
                    await NavigateFromAsync(false);
                }
            };
            FrameFacade.Navigated += (s, e) =>
            {
                NavigateTo(e.NavigationMode, e.Parameter);
            };
        }

        // before navigate (cancellable)
        bool NavigatingFrom(bool suspending)
        {
            var page = FrameFacade.Content as Page;
            if (page != null)
            {
                var dataContext = page.DataContext as INavigable;
                if (dataContext != null)
                {
                    var args = new NavigatingEventArgs
                    {
                        PageType = FrameFacade.CurrentPageType,
                        Parameter = FrameFacade.CurrentPageParam,
                        Suspending = suspending,
                    };
                    dataContext.OnNavigatingFrom(args);
                    return !args.Cancel;
                }
            }
            return true;
        }

        // after navigate
        async Task NavigateFromAsync(bool suspending)
        {
            var page = FrameFacade.Content as Page;
            if (page != null)
            {
                // call viewmodel
                var dataContext = page.DataContext as INavigable;
                if (dataContext != null)
                {
                    dataContext.Identifier = string.Format("Page- {0}", FrameFacade.BackStackDepth);
                    var pageState = FrameFacade.PageStateContainer(page.GetType());
                    await dataContext.OnNavigatedFromAsync(pageState, suspending);
                }
            }
        }

        void NavigateTo(NavigationMode mode, string parameter)
        {
            LastNavigationParameter = parameter;
            LastNavigationType = FrameFacade.Content.GetType().FullName;

            if (mode == NavigationMode.New)
            {
                FrameFacade.ClearFrameState();
            }

            var page = FrameFacade.Content as Page;
            if (page != null)
            {
                // call viewmodel
                var dataContext = page.DataContext as INavigable;
                if (dataContext != null)
                {
                    if (dataContext.Identifier != null
                        && (mode == NavigationMode.Forward || mode == NavigationMode.Back))
                    {
                        // don't call load if cached && navigating back/forward
                        return;
                    }
                    else
                    {
                        // prepare for state load
                        dataContext.NavigationService = this;
                        var pageState = FrameFacade.PageStateContainer(page.GetType());
                        dataContext.OnNavigatedTo(parameter, mode, pageState);
                    }
                }
            }
        }

        // TODO: this will spawn a new window instead of navigating to an existing frame.
        public async Task<int> OpenAsync(Type page, string parameter = null, ViewSizePreference size = ViewSizePreference.UseHalf)
        {
            var coreView = CoreApplication.CreateNewView();
            ApplicationView view = null;
            var create = new Action(() =>
            {
                // setup content
                var frame = new Frame();
                frame.NavigationFailed += (s, e) => { System.Diagnostics.Debugger.Break(); };
                frame.Navigate(page, parameter);

                // create window
                var window = Window.Current;
                window.Content = frame;

                // setup view/collapse
                view = ApplicationView.GetForCurrentView();
                Windows.Foundation.TypedEventHandler<ApplicationView, ApplicationViewConsolidatedEventArgs> consolidated = null;
                consolidated = new Windows.Foundation.TypedEventHandler<ApplicationView, ApplicationViewConsolidatedEventArgs>((s, e) =>
                {
                    (s as ApplicationView).Consolidated -= consolidated;
                    if (CoreApplication.GetCurrentView().IsMain)
                        return;
                    try { window.Close(); }
                    finally { CoreApplication.GetCurrentView().CoreWindow.Activate(); }
                });
                view.Consolidated += consolidated;
            });

            // execute create
            await WindowWrapper.Current().Dispatcher.DispatchAsync(create);

            // show view
            if (await ApplicationViewSwitcher.TryShowAsStandaloneAsync(view.Id, size))
            {
                // change focus
                await ApplicationViewSwitcher.SwitchAsync(view.Id);
            }
            return view.Id;
        }

        public bool Navigate(Type page, object parameter = null)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));
            if (page.FullName.Equals(LastNavigationType)
                && parameter == LastNavigationParameter)
                return false;
            return FrameFacade.Navigate(page, parameter);
        }

        public void SaveNavigation()
        {
            var state = FrameFacade.PageStateContainer(GetType());
            if (state == null)
            {
                throw new InvalidOperationException("State container is unexpectedly null");
            }

            state["CurrentPageType"] = CurrentPageType?.ToString();
            state["CurrentPageParam"] = CurrentPageParam;
            state["NavigateState"] = FrameFacade?.GetNavigationState();
        }

        public bool RestoreSavedNavigation()
        {
            try
            {
                var state = FrameFacade.PageStateContainer(GetType());
                FrameFacade.CurrentPageType = Type.GetType(state["CurrentPageType"].ToString());
                FrameFacade.CurrentPageParam = state["CurrentPageParam"]?.ToString();
                FrameFacade.SetNavigationState(state["NavigateState"].ToString());
                NavigateTo(NavigationMode.Refresh, FrameFacade.CurrentPageParam);
                return true;
            }
            catch { return false; }
        }

        public void Refresh() { FrameFacade.Refresh(); }

        public void GoBack() { if (FrameFacade.CanGoBack) FrameFacade.GoBack(); }

        public bool CanGoBack { get { return FrameFacade.CanGoBack; } }

        public void GoForward() { FrameFacade.GoForward(); }

        public bool CanGoForward { get { return FrameFacade.CanGoForward; } }

        public void ClearHistory() { FrameFacade.Frame.BackStack.Clear(); }

        public void Resuming() { /* nothing */ }

        public async Task SuspendingAsync()
        {
            SaveNavigation();
            await NavigateFromAsync(true);
        }

        public void Show(SettingsFlyout flyout, string parameter = null)
        {
            if (flyout == null)
                throw new ArgumentNullException(nameof(flyout));
            var dataContext = flyout.DataContext as INavigable;
            if (dataContext != null)
            {
                dataContext.OnNavigatedTo(parameter, NavigationMode.New, null);
            }
            flyout.Show();
        }

        public Type CurrentPageType { get { return FrameFacade.CurrentPageType; } }
        public string CurrentPageParam { get { return FrameFacade.CurrentPageParam; } }
    }
}

