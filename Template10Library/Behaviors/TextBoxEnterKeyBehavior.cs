﻿using Microsoft.Xaml.Interactivity;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;

namespace Template10.Behaviors
{
    [ContentProperty(Name = nameof(Actions))]
    [TypeConstraint(typeof(TextBox))]
    public class TextBoxEnterKeyBehavior : DependencyObject, IBehavior
    {
        private TextBox AssociatedTextBox { get { return AssociatedObject as TextBox; } }
        public DependencyObject AssociatedObject { get; private set; }

        public void Attach(DependencyObject associatedObject)
        {
            AssociatedObject = associatedObject;
            AssociatedTextBox.KeyDown += AssociatedTextBox_KeyDown;
        }

        public void Detach()
        {
            AssociatedTextBox.KeyDown -= AssociatedTextBox_KeyDown;
        }

        private void AssociatedTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Interaction.ExecuteActions(AssociatedObject, this.Actions, null);
            }
        }

        public ActionCollection Actions
        {
            get
            {
                var actions = (ActionCollection)base.GetValue(ActionsProperty);
                if (actions == null)
                {
                    base.SetValue(ActionsProperty, actions = new ActionCollection());
                }
                return actions;
            }
        }
        public static readonly DependencyProperty ActionsProperty = 
            DependencyProperty.Register("Actions", typeof(ActionCollection), 
                typeof(TextBoxEnterKeyBehavior), new PropertyMetadata(null));
    }
}
