using StupidSimpleUpdater.Updater;
using StupidSimpleUpdater.Utilities;
using System.ComponentModel;
using System.Windows.Controls;

namespace StupidSimpleUpdater.DataContext
{
    public class UpdaterWindowContext : INotifyPropertyChanged
    {
        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region fields/properties

        private Page currentPage;
        public Page CurrentPage
        {
            get => currentPage;
            set
            {
                if (currentPage != value)
                {
                    currentPage = value;
                    NotifyPropertyChanged(nameof(CurrentPage));
                }
            }
        }

        private string windowTitle;
        public string WindowTitle
        {
            get => windowTitle;
            set
            {
                if (windowTitle != value)
                {
                    windowTitle = value;
                    NotifyPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        #endregion

        #region .ctor

        public UpdaterWindowContext()
        {
            UpdateManager updateManager;

            if (!App.Current.Properties.Contains(App.UPDATE_MANAGER))
            {
                Logging.Error(string.Format("UpdateManager could not be found in global properties while initializing {0}.", GetType().ToString()));
                App.Current.Shutdown();
            }
            updateManager = (UpdateManager)App.Current.Properties[App.UPDATE_MANAGER];

            WindowTitle = updateManager.ManifestBundle.Name + " Updater";
        }

        #endregion

        #region event methods

        public void NotifyPropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }
}
