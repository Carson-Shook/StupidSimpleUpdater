using StupidSimpleUpdater.DataContext;
using System.Windows;
using System.Windows.Controls;

namespace StupidSimpleUpdater.View
{
    /// <summary>
    /// Interaction logic for ErrorPage.xaml
    /// </summary>
    public partial class ErrorPage : Page
    {
        public ErrorPage(string errorText)
        {
            InitializeComponent();

            ((ErrorPageContext)this.DataContext).ErrorText = errorText;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }
    }
}
