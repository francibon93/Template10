﻿using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Template10.Common
{
    public static class XamlHelper
    {
        public static List<T> AllChildren<T>(DependencyObject parent) where T : Control
        {
            var list = new List<T>();
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T)
                    list.Add(child as T);
                list.AddRange(AllChildren<T>(child));
            }
            return list;
        }
    }
}
