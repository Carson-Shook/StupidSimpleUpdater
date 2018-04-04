using StupidSimpleUpdater.Updater;
using StupidSimpleUpdater.Utilities;
using System.ComponentModel;
using System.Windows.Controls;

namespace StupidSimpleUpdater.DataContext
{
    public class ErrorPageContext : INotifyPropertyChanged
    {
        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region fields/properties

        private string errorText;
        public string ErrorText
        {
            get => errorText;
            set
            {
                if (errorText != value)
                {
                    errorText = value;
                    NotifyPropertyChanged(nameof(ErrorText));
                }
            }
        }

        #endregion

        #region .ctor

        public ErrorPageContext()
        { }

        #endregion

        #region event methods

        public void NotifyPropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion
    }
}
