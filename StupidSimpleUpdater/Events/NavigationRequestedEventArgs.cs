using System;
using System.Windows.Controls;

namespace StupidSimpleUpdater.Events
{
    public class NavigationRequestedEventArgs : EventArgs
    {
        #region fields/properties

        private readonly Page page;
        public Page Page { get => page; }

        #endregion

        #region .ctor

        public NavigationRequestedEventArgs(Page page)
        {
            this.page = page;
        }

        #endregion
    }
}
