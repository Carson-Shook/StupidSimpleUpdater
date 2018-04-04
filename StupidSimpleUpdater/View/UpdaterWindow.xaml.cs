using StupidSimpleUpdater.DataContext;
using StupidSimpleUpdater.Events;
using StupidSimpleUpdater.Utilities;
using System;
using System.Windows;
using System.Windows.Controls;

namespace StupidSimpleUpdater.View
{
    /// <summary>
    /// Interaction logic for UpdaterWindow.xaml
    /// </summary>
    public partial class UpdaterWindow : Window
    {
        #region .ctor

        public UpdaterWindow()
        {
            InitializeComponent();
            
            Navigation.NavigationRequested += OnNavigationRequested;

            NavigateToPage(new UpdateFoundPrompt());

            this.Show();
        }

        #endregion

        #region methods

        private void NavigateToPage(Page page)
        {
            if (page != null)
            {
                ((UpdaterWindowContext)this.DataContext).CurrentPage = page;
            }
        }

        #endregion

        #region event handlers

        private void OnNavigationRequested(object sender, EventArgs e)
        {
            if (e != null)
            {
                Page page = ((NavigationRequestedEventArgs)e).Page;
                NavigateToPage(page);
            }
        }

        #endregion
    }
}