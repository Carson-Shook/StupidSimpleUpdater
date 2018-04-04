using System;
using System.Windows.Controls;

namespace StupidSimpleUpdater.Events
{
    public class InstallFailedEventArgs : EventArgs
    {
        #region fields/properties

        private readonly string message;
        public string Message { get => message; }

        #endregion

        #region .ctor

        public InstallFailedEventArgs(string message)
        {
            this.message = message;
        }

        #endregion
    }
}
