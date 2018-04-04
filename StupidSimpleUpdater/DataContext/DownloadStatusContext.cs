using StupidSimpleUpdater.Events;
using StupidSimpleUpdater.Updater;
using StupidSimpleUpdater.Utilities;
using System;
using System.ComponentModel;
using System.Net;

namespace StupidSimpleUpdater.DataContext
{
    public class DownloadStatusContext : INotifyPropertyChanged
    {
        #region events

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler InstallFailed;

        #endregion

        #region fields/properties

        private UpdateManager updateManager;

        private double downloadProgress = 0d;
        public double DownloadProgress
        {
            get => downloadProgress;
            set
            {
                if (downloadProgress != value)
                {
                    downloadProgress = value;
                    NotifyPropertyChanged(nameof(DownloadProgress));
                }
            }
        }

        #endregion

        #region .ctor

        public DownloadStatusContext()
        {
            if (!App.Current.Properties.Contains(App.UPDATE_MANAGER))
            {
                Logging.Error(string.Format("UpdateManager could not be found in global properties while initializing {0}.", GetType().ToString()));
                App.Current.Shutdown();
            }
            updateManager = (UpdateManager)App.Current.Properties[App.UPDATE_MANAGER];

            updateManager.TryDownloadInstaller(updateManager.ManifestBundle.DownloadUrl, OnDownloadProgressChanged, OnDownloadFileCompleted);
        }

        #endregion

        #region event methods

        public void NotifyPropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        #region event handlers

        private void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            DownloadProgress = percentage;
        }

        private void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (!updateManager.TryLaunchInstaller())
                {
                    InstallFailed?.Invoke(this, new InstallFailedEventArgs("Install Error:\r\nA problem occured while trying to install this update.\r\nPlease try reinstalling the software, or contact the developer."));
                }
                else
                {
                    App.Current.Shutdown();
                }
            }
            else
            {
                Logging.Error(string.Format("The following error occured while trying to download the update:\r\n\t{0}", e.Error.Message));
                InstallFailed?.Invoke(this, new InstallFailedEventArgs("Download Error:\r\nA problem occured while trying to download this update.\r\nPlease check your connection and try updating again."));
            }
        }

        #endregion
    }
}
