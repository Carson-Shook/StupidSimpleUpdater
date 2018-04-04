using StupidSimpleUpdater.Events;
using System;
using System.Windows.Controls;

namespace StupidSimpleUpdater.Utilities
{
    public static class Navigation
    {
        #region events

        public static event EventHandler NavigationRequested;
        public static event EventHandler CloseRequested;

        #endregion

        #region static methods

        public static void NotifyNavigationRequested(object context, Page page)
        {
            NavigationRequested?.Invoke(context, new NavigationRequestedEventArgs(page));
        }

        public static void NotifyCloseRequested(object context)
        {
            CloseRequested?.Invoke(context, new EventArgs());
        }

        #endregion
    }
}
