using StupidSimpleUpdater.DataContext;
using StupidSimpleUpdater.Events;
using StupidSimpleUpdater.Utilities;
using System;
using System.Windows.Controls;

namespace StupidSimpleUpdater.View
{
    /// <summary>
    /// Interaction logic for DownloadStatus.xaml
    /// </summary>
    public partial class DownloadStatus : Page
    {
        #region .ctor

        public DownloadStatus()
        {
            InitializeComponent();

            ((DownloadStatusContext)this.DataContext).InstallFailed += OnInstallFailed;
        }

        #endregion

        #region event handlers

        private void OnInstallFailed(object sender, EventArgs e)
        {
            if (e != null)
            {
                string errorMessage = ((InstallFailedEventArgs)e).Message;
                Navigation.NotifyNavigationRequested(this, new ErrorPage(errorMessage));
            }
        }

        #endregion
    }
}
