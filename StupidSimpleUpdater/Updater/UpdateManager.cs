using StupidSimpleUpdater.Utilities;
using StupidSimpleUpdater.Updater.XmlData;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.ComponentModel;
using System.Threading;

namespace StupidSimpleUpdater.Updater
{
    public class UpdateManager
    {
        #region constants

        private const string TEMP_MSI_NAME = "updaterTemp.msi";
        private const string FALLBACK_PATCH_NOTES = 
            @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033{\fonttbl{\f0\fnil\fcharset0 Calibri;} }" +
            @"\r\n{\*\generator Riched20 10.0.16299 }\viewkind4\uc1 " +
            @"\r\n\pard\sa200\sl276\slmult1\b\f0\fs28\lang9 Error:\b0  Patch notes could not be loaded.\par\r\n }";

        #endregion

        #region fields/properties

        // ApplicationInvoked is used can be referenced to
        // determine whether or not certain checks should be made.
        private bool applicationInvoked;
        public bool ApplicationInvoked { get => applicationInvoked; }

        // CheckInfo is the regularly updated file stored in ProgramData
        // that tracks the last check date, and whether or not the
        // user has elected to skip this version.
        private CheckInfo checkInfo;
        public CheckInfo CheckInfo { get => checkInfo; }

        // The SSUPConfig contains information about where to check for
        // updates, and what update bundle we want to download.
        private SSUPConfig ssupConfig;
        public SSUPConfig SSUPConfig { get => ssupConfig; }

        // The ManifestBundle is the correct bundle for a given build
        // based on the BuildType in the SSUPConfig file. It contains
        // Version info, patchNotes URL, and MSI URL.
        private SSUPUpdateManifestBundle manifestBundle;
        public SSUPUpdateManifestBundle ManifestBundle { get => manifestBundle; }

        // Patch Notes rich text document as a byte array.
        private byte[] patchNotesBytes;
        public byte[] PatchNotesBytes { get => patchNotesBytes; }

        // Fallback patch notes that can be used when the expected
        // patch notes fail to download, or are improperly formatted.
        public byte[] FallbackPatchNotes { get => System.Text.Encoding.Default.GetBytes(FALLBACK_PATCH_NOTES); }

        #endregion

        #region .ctor

        public UpdateManager()
        {
            this.applicationInvoked = false;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        public UpdateManager(bool applicationInvoked)
        {
            this.applicationInvoked = applicationInvoked;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
        }

        #endregion

        #region public methods

        public bool TryCheckForUpdates()
        {
            if (!TryGetConfig(out ssupConfig))
            {
                return false;
            }

            Logging.Info(string.Format("Successfully read {0}", SSUPConfig.SSUP_CONFIG_FILENAME));

            if (!TryGetCheckInfo(SSUPConfig.Name, out checkInfo))
            {
                return false;
            }

            Logging.Info(string.Format("{0} checked", CheckInfo.CHECK_INFO_FILENAME));

            if (ApplicationInvoked && !checkInfo.TimeHasElapsed(SSUPConfig.CheckFrequency))
            {
                return false;
            }

            Logging.Info("Enough time elapsed since last check...");

            if (!TryDownloadUpdateManifest(SSUPConfig.Url, out SSUPUpdateManifest manifest))
            {
                return false;
            }

            manifestBundle = manifest[SSUPConfig.BuildType];

            Logging.Info(string.Format("Downloaded update manifest {0} from {1}", SSUPConfig.BuildType, SSUPConfig.Url));

            if (ManifestBundle == null)
            {
                Logging.Error(string.Format("{0} update bundle could not be located. Make sure that it is available and the element is spelled correctly", SSUPConfig.BuildType));
                return false;
            }

            if (ApplicationInvoked
                && checkInfo.SkipThisVersion
                && checkInfo.LatestVersion != null
                && checkInfo.LatestVersion.ToString().Equals(ManifestBundle.CurrentVersion.ToString()))
            {
                Logging.Info(string.Format("User opted to skip version {0}", checkInfo.LatestVersion.ToString()));
                checkInfo.UpdateLastCheckDate();
                checkInfo.Write();
                return false;
            }

            string applicationLocation = AppDomain.CurrentDomain.BaseDirectory + SSUPConfig.Name + ".exe";
            try
            {
                FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(applicationLocation);
                Version parseVersion = Version.Parse(fileVersionInfo.FileVersion);

                if (ManifestBundle.CurrentVersion.CompareTo(parseVersion) <= 0)
                {
                    Logging.Info(string.Format("Installed version ({0}) of {1} is up to date.", ManifestBundle.CurrentVersion.ToString(), SSUPConfig.Name));
                    checkInfo.UpdateLastCheckDate();
                    checkInfo.LatestVersion = manifestBundle.CurrentVersion;
                    checkInfo.Write();
                    return false;
                }
            }
            catch (FileNotFoundException)
            {
                Logging.Error(string.Format("The executable name listed in the SSUPConfig file could not be found at: \r\n\t{0}", applicationLocation));
                return false;
            }
            catch (ArgumentNullException)
            {
                Logging.Warning(string.Format("No file version information could be found for: \r\n\t{0}. We'll assume the developer just forgot the first time. Continuing anyway.", applicationLocation));
            }
            catch (Exception e)
            {
                Logging.Warning(string.Format("The file version was not formatted correctly for: \r\n\t{0}, leading to the following error: \r\n\t{1}\r\n\tIf you're the developer of this application, you should check the formatting of your FileVersion. \r\n\tContinuing anyway.", applicationLocation, e.Message));
            }
            finally
            {
                checkInfo.LatestVersion = manifestBundle.CurrentVersion;
            }

            Logging.Info(string.Format("Version {0} is available.", checkInfo.LatestVersion));

            if (!TryDownloadPatchNotes(manifestBundle.PatchNotesUrl, out patchNotesBytes))
            {
                Logging.Info("Using fallback patchnotes.");
                patchNotesBytes = FallbackPatchNotes;
            }
            else
            {
                Logging.Info("Got patchnotes.");
            }

            return true;
        }

        public bool TryDownloadInstaller(string url,
            Action<object, DownloadProgressChangedEventArgs> progressChanged,
            Action<object, AsyncCompletedEventArgs> downloadFinished)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progressChanged);
                    webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadFinished);

                    FileInfo fileInfo = new FileInfo(CheckInfo.ProgramDataDir + SSUPConfig.Name + @"\" + TEMP_MSI_NAME);

                    if (!Directory.Exists(fileInfo.Directory.ToString()))
                    {
                        fileInfo.Directory.Create();
                    }

                    webClient.DownloadFileAsync(new Uri(url), fileInfo.ToString());
                }
            }
            catch (Exception e)
            {
                Logging.Error(string.Format("The following error occured while trying to download the update: \r\n\t{0}", e.Message));
                return false;
            }

            return true;
        }

        public bool TryLaunchInstaller()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName(SSUPConfig.Name))
                {
                    process.Kill();
                }

                Thread.Sleep(1000);

                Process.Start(CheckInfo.ProgramDataDir + SSUPConfig.Name + @"\" + TEMP_MSI_NAME);

                Thread.Sleep(500);
            }
            catch (Exception e)
            {
                Logging.Error(string.Format("The following error occured while trying to start the update: \r\n\t{0}", e.Message));
                return false;
            }
            return true;
        }

        #endregion

        #region private methods

        private bool TryGetConfig(out SSUPConfig ssupConfig)
        {
            ssupConfig = null;
            string configPath = AppDomain.CurrentDomain.BaseDirectory + SSUPConfig.SSUP_CONFIG_FILENAME;

            if (!File.Exists(configPath))
            {
                Logging.Warning(string.Format("{0} could not be found.\r\n\tIt has either been deleted or was not added to this application's installer.", configPath));
                return false;
            }

            ssupConfig = new SSUPConfig(configPath);

            if (ssupConfig == null)
            {
                Logging.Error(string.Format("{0} somehow failed to initialize.", nameof(ssupConfig)));
                return false;
            }

            if (!ssupConfig.IsValid())
            {
                Logging.Error(string.Format("{0} is not valid.\r\n\tIt was improperly configured, and this application may need to be reinstalled to fix it.", configPath));
                ssupConfig = null;
                return false;
            }

            return true;
        }

        private bool TryGetCheckInfo(string assemblyName, out CheckInfo checkInfo)
        {
            if (assemblyName == null)
            {
                // We want to throw an exception here since a null
                // would mean that I, the developer screwed up.
                throw new ArgumentNullException(nameof(assemblyName));
            }

            string checkInfoPath = CheckInfo.ProgramDataDir + assemblyName + @"\" + CheckInfo.CHECK_INFO_FILENAME;

            checkInfo = new CheckInfo(checkInfoPath);

            if (checkInfo == null)
            {
                Logging.Error(string.Format("{0} somehow failed to initialize.", nameof(checkInfo)));
                return false;
            }

            // CheckInfo doesn't have an IsValid method because it will always be recreated anyway.

            return true;
        }

        private bool TryDownloadUpdateManifest(string url, out SSUPUpdateManifest manifest)
        {
            manifest = null;
            string xmlText;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    xmlText = webClient.DownloadString(url);
                }
            }
            catch (Exception e)
            {
                Logging.Error(string.Format("The following error occured while trying to download the Update Manifest: \r\n\t{0}", e.Message));
                return false;
            }

            manifest = new SSUPUpdateManifest(xmlText);

            if (manifest == null)
            {
                Logging.Error(string.Format("{0} somehow failed to initialize.", nameof(manifest)));
                return false;
            }

            return true;
        }

        private bool TryDownloadPatchNotes(string url, out byte[] patchNotes)
        {
            patchNotes = null;

            try
            {
                using (WebClient webClient = new WebClient())
                {
                    patchNotes = webClient.DownloadData(url);
                }
            }
            catch (Exception e)
            {
                Logging.Error(string.Format("The following error occured while trying to download the Patch Notes: \r\n\t{0}", e.Message));
                return false;
            }

            return true;
        }

        #endregion
    }
}
