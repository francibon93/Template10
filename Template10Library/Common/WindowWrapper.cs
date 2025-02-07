﻿using System;
using System.Collections.Generic;
using System.Linq;
using Template10.Services.NavigationService;
using Windows.UI.Xaml;

namespace Template10.Common
{
    public class WindowWrapper
    {
        public static WindowWrapper Default() { return ActiveWrappers.FirstOrDefault(); }
        public readonly static List<WindowWrapper> ActiveWrappers = new List<WindowWrapper>();
        public static WindowWrapper Current() { return ActiveWrappers.FirstOrDefault(x => x.Window == Window.Current) ?? Default(); }
        public static WindowWrapper Current(Window window) { return ActiveWrappers.FirstOrDefault(x => x.Window == window); }
        public static WindowWrapper Current(NavigationService nav) { return ActiveWrappers.First(x => x.NavigationServices.Contains(nav)); }

        public WindowWrapper(Window window)
        {
            if (ActiveWrappers.Any(x => x.Window == window))
                throw new Exception("Windows already has a wrapper; use Current(window) to fetch.");
            Window = window;
            ActiveWrappers.Add(this);
            Dispatcher = new DispatcherWrapper(window.Dispatcher);
            window.Closed += (s, e) => { ActiveWrappers.Remove(this); };
        }

        public void Close() { Window.Close(); }
        public Window Window { get; private set; }
        public DispatcherWrapper Dispatcher { get; private set; }
        public List<NavigationService> NavigationServices { get; } = new List<NavigationService>();
    }
}
