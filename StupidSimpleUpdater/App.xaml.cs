using StupidSimpleUpdater.Updater;
using StupidSimpleUpdater.Utilities;
using StupidSimpleUpdater.View;
using System.Windows;

namespace StupidSimpleUpdater
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region constants

        // The "-appinv" argument should be used when the updater
        // is invoked by a program, as opposed to a user
        private const string APPLICATION_INVOKED_ARG = "-appinv";

        // easy UpdateManager reference
        public const string UPDATE_MANAGER = "UpdateManager";

        #endregion

        #region event methods

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Logging.ClearLog();

            bool applicationInvoked = false;
            if (e.Args.Length > 0)
            {
                applicationInvoked = e.Args[0].Equals(APPLICATION_INVOKED_ARG);
            }
            Logging.Info(string.Format("Updater started by {0}", applicationInvoked ? "Application" : "User"));

            UpdateManager updateManager = new UpdateManager(applicationInvoked);

            if (!updateManager.TryCheckForUpdates())
            {
                // No update. Pack it up; we're through here!
                Application.Current.Shutdown();
                return;
            }

            Current.Properties.Add(UPDATE_MANAGER, updateManager);

            new UpdaterWindow();
        }

        #endregion
    }
}
