using StupidSimpleUpdater.DataContext;
using StupidSimpleUpdater.Utilities;
using System.Windows;
using System.Windows.Controls;

namespace StupidSimpleUpdater.View
{
    /// <summary>
    /// Interaction logic for UpdateFoundPrompt.xaml
    /// </summary>
    public partial class UpdateFoundPrompt : Page
    {
        #region .ctor

        public UpdateFoundPrompt()
        {
            InitializeComponent();
        }

        #endregion

        #region event handlers

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            Navigation.NotifyNavigationRequested(this, new DownloadStatus());
        }

        private void PostponeButton_Click(object sender, RoutedEventArgs e)
        {
            ((UpdateFoundPromptContext)this.DataContext).Postpone();
        }

        #endregion
    }
}