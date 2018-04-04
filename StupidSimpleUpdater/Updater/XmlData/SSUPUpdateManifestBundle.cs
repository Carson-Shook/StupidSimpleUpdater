using StupidSimpleUpdater.Utilities;
using System;
using System.Xml;

namespace StupidSimpleUpdater.Updater.XmlData
{
    public class SSUPUpdateManifestBundle
    {
        #region constants

        public const string NAME_NODE = "Name";
        public const string CURRENT_VERSION_NODE = "CurrentVersion";
        public const string PATCH_NOTES_URL_NODE = "PatchNotesUrl";
        public const string DOWNLOAD_URL_NODE = "DownloadUrl";

        #endregion

        #region fields/properties

        // Customer friendly name of the program
        private string name;
        public string Name { get => name; }

        // The current version of the application that is
        // avaiable in this bundle.
        private Version currentVersion;
        public Version CurrentVersion { get => currentVersion; }

        // The url of the patchnotes RTF document for this bundle.
        private string patchNotesUrl;
        public string PatchNotesUrl { get => patchNotesUrl; }

        // The url of the msi update
        private string downloadUrl;
        public string DownloadUrl { get => downloadUrl; }

        #endregion

        #region .ctor

        public SSUPUpdateManifestBundle(XmlNode parentNode)
        {
            if (parentNode == null)
            {
                // Seriously, if there is a null XmlNode here, then I
                // am seriously screwing up as a developer.
                throw new NullReferenceException(nameof(parentNode));
            }

            foreach (XmlNode node in parentNode.ChildNodes)
            {
                switch (node.Name)
                {
                    case NAME_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                Logging.Error(string.Format("{0} node is empty in the {1} update bundle.", NAME_NODE, parentNode.Name));
                                break;
                            }
                            name = node.InnerText;
                        }
                        break;
                    case CURRENT_VERSION_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                Logging.Error(string.Format("{0} node is empty in the {1} update bundle.", CURRENT_VERSION_NODE, parentNode.Name));
                                break;
                            }
                            Version parseValue;
                            if (!Version.TryParse(node.InnerText, out parseValue))
                            {
                                Logging.Error(string.Format("Value of {0} node in the {1} update bundle could not be parsed into a valid version number.\r\n\tValue found looked like: {2}", CURRENT_VERSION_NODE, parentNode.Name, node.InnerText));
                            }
                            currentVersion = parseValue;
                        }
                        break;
                    case PATCH_NOTES_URL_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                Logging.Error(string.Format("{0} node is empty in the {1} update bundle.", PATCH_NOTES_URL_NODE, parentNode.Name));
                                break;
                            }
                            string tempUriString = node.InnerText;
                            if (!Uri.IsWellFormedUriString(tempUriString, UriKind.Absolute))
                            {
                                try
                                {
                                    // Maybe the dev just forgot to escape the URL
                                    tempUriString = Uri.EscapeUriString(tempUriString);
                                    if (!Uri.IsWellFormedUriString(tempUriString, UriKind.Absolute))
                                    {
                                        // Or maybe they just didn't supply a valid URL at all...
                                        Logging.Error(string.Format("{0} node does not contain a valid URL. Cannot locate patch notes for update bundle {1}.", PATCH_NOTES_URL_NODE, parentNode.Name));
                                        break;
                                    }
                                }
                                catch (UriFormatException)
                                {
                                    Logging.Error(string.Format("Specified update URL is either too long or improperly formatted for {0} node in update bundle {1}.", PATCH_NOTES_URL_NODE, parentNode.Name));
                                    break;
                                }
                                catch (Exception e)
                                {
                                    Logging.Error(string.Format("The following error occured while trying to read the value of {0} in upddate bundle {1}: \r\n\t{2}", PATCH_NOTES_URL_NODE, parentNode.Name, e.Message));
                                    break;
                                }
                            }
                            patchNotesUrl = tempUriString;
                        }
                        break;
                    case DOWNLOAD_URL_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                Logging.Error(string.Format("{0} node is empty in the {1} update bundle.", DOWNLOAD_URL_NODE, parentNode.Name));
                                break;
                            }
                            string tempUriString = node.InnerText;
                            if (!Uri.IsWellFormedUriString(tempUriString, UriKind.Absolute))
                            {
                                try
                                {
                                    // Maybe the dev just forgot to escape the URL
                                    tempUriString = Uri.EscapeUriString(tempUriString);
                                    if (!Uri.IsWellFormedUriString(tempUriString, UriKind.Absolute))
                                    {
                                        // Or maybe they just didn't supply a valid URL at all...
                                        Logging.Error(string.Format("{0} node does not contain a valid URL. Cannot locate patch notes for update bundle {1}.", DOWNLOAD_URL_NODE, parentNode.Name));
                                        break;
                                    }
                                }
                                catch (UriFormatException)
                                {
                                    Logging.Error(string.Format("Specified update URL is either too long or improperly formatted for {0} node in update bundle {1}.", DOWNLOAD_URL_NODE, parentNode.Name));
                                    break;
                                }
                                catch (Exception e)
                                {
                                    Logging.Error(string.Format("The following error occured while trying to read the value of {0} in upddate bundle {1}: \r\n\t{2}", DOWNLOAD_URL_NODE, parentNode.Name, e.Message));
                                    break;
                                }
                            }
                            downloadUrl = tempUriString;
                        }
                        break;
                }
            }
        }

        #endregion

        #region methods

        public bool IsValid()
        {
            return (!string.IsNullOrEmpty(name) && currentVersion != null && !string.IsNullOrEmpty(patchNotesUrl) && !string.IsNullOrEmpty(downloadUrl));
        }

        #endregion
    }
}
