using StupidSimpleUpdater.Updater;
using StupidSimpleUpdater.Utilities;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace StupidSimpleUpdater.DataContext
{
    public class UpdateFoundPromptContext : INotifyPropertyChanged
    {
        #region events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region fields/properties

        private UpdateManager updateManager;

        private string updateAvailableText = "Version [Unknown] of [Your Software] is available to download and install.";
        public string UpdateAvailableText
        {
            get => updateAvailableText;
            set
            {
                if (updateAvailableText != value)
                {
                    updateAvailableText = value;
                    NotifyPropertyChanged(nameof(UpdateAvailableText));
                }
            }
        }

        private FlowDocument patchNotes = new FlowDocument();
        public FlowDocument PatchNotes
        {
            get => patchNotes;
            set
            {
                if (patchNotes != value)
                {
                    patchNotes = value;
                    NotifyPropertyChanged(nameof(PatchNotes));
                }
            }
        }

        private bool skipThisVersion;
        public bool SkipThisVersion
        {
            get => skipThisVersion;
            set
            {
                if (skipThisVersion != value)
                {
                    skipThisVersion = value;
                    NotifyPropertyChanged(nameof(SkipThisVersion));
                }
            }
        }

        #endregion

        #region .ctor

        public UpdateFoundPromptContext()
        {
            if (!App.Current.Properties.Contains(App.UPDATE_MANAGER))
            {
                Logging.Error(string.Format("UpdateManager could not be found in global properties while initializing {0}.", GetType().ToString()));
                App.Current.Shutdown();
            }
            updateManager = (UpdateManager)App.Current.Properties[App.UPDATE_MANAGER];

            UpdateAvailableText = "Version "
                        + updateManager.ManifestBundle.CurrentVersion.ToString()
                        + " of "
                        + updateManager.ManifestBundle.Name
                        + " is available to download and install.";

            // if the background color isn't set now, then it won't set at all.
            PatchNotes.Background = SystemColors.WindowBrush;

            try
            {
                TextRange textRange = new TextRange(PatchNotes.ContentStart, PatchNotes.ContentEnd);
                MemoryStream memoryStream = new MemoryStream(updateManager.PatchNotesBytes);
                textRange.Load(memoryStream, DataFormats.Rtf);
            }
            catch (Exception e)
            {
                Logging.Warning(string.Format("Using fallback patch notes because the following error occured while trying to display the real ones:\r\n\t{0}", e.Message));
                TextRange textRange = new TextRange(PatchNotes.ContentStart, PatchNotes.ContentEnd);
                MemoryStream memoryStream = new MemoryStream(updateManager.FallbackPatchNotes);
                textRange.Load(memoryStream, DataFormats.Rtf);
            }
        }

        #endregion

        #region event methods

        public void NotifyPropertyChanged(string propName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        #region methods

        public void Postpone()
        {
            updateManager.CheckInfo.UpdateLastCheckDate();
            updateManager.CheckInfo.SkipThisVersion = SkipThisVersion;
            updateManager.CheckInfo.Write();

            App.Current.Shutdown();
        }

        #endregion
    }
}
