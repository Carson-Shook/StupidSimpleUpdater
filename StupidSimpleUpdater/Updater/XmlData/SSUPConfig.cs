using StupidSimpleUpdater.Utilities;
using System;
using System.IO;
using System.Xml;

namespace StupidSimpleUpdater.Updater.XmlData
{
    public class SSUPConfig
    {
        #region constants

        public const string SSUP_CONFIG_FILENAME = "SSUPConfig.xml";
        public const string BUILD_TYPE_NODE = "BuildType";
        public const string NAME_NODE = "Name";
        public const string CHECK_FREQUENCY_NODE = "CheckFrequency";
        public const string URL_NODE = "Url";

        #endregion

        #region fields/properties

        // BuildType is the name of the update bundle element you wish to check
        private string buildType;
        public string BuildType { get => buildType; }

        // Name of the assembly, and consequently the directory created
        // in the StupidSimpleUpdater ProgramData directory
        private string name;
        public string Name { get => name; }

        // The frequency that SSUP should check for updates when run with the -appinv flag
        private CheckFrequencies checkFrequency = CheckFrequencies.Daily;
        public CheckFrequencies CheckFrequency { get => checkFrequency; }

        // URL of the SSUPUpdateManifest.xml file.
        private string url;
        public string Url { get => url; }

        #endregion

        #region .ctor

        public SSUPConfig(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                Logging.Error(string.Format("No path specified for {0}.", SSUP_CONFIG_FILENAME));
                return;
            }

            XmlDocument configDoc = new XmlDocument();
            try
            {
                configDoc.Load(configPath);
            }
            catch (FileNotFoundException)
            {
                Logging.Error(string.Format("{0} could not be found at the following location: {1}.", SSUP_CONFIG_FILENAME, configPath));
                return;
            }
            catch (XmlException)
            {
                Logging.Error(string.Format("There was a problem with the format of your {0} file.", SSUP_CONFIG_FILENAME));
                return;
            }
            catch (Exception e)
            {
                Logging.Error(string.Format("The following error occured while trying to load {0}: \r\n\t{1}", SSUP_CONFIG_FILENAME, e.Message));
                return;
            }

            foreach (XmlNode node in configDoc.DocumentElement.ChildNodes)
            {
                switch (node.Name)
                {
                    case BUILD_TYPE_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                Logging.Error(string.Format("{0} node is empty. Ensure that {1} contains a value for {0}.", BUILD_TYPE_NODE, SSUP_CONFIG_FILENAME));
                                break;
                            }
                            if (!XmlUtilities.IsValidXmlNodeName(node.InnerText))
                            {
                                Logging.Error(string.Format("Value of {0} node contains invalid characters. Ensure that {0} is a valid XML element name.", BUILD_TYPE_NODE));
                                break;
                            }
                            buildType = node.InnerText;
                        }
                        break;
                    case NAME_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                Logging.Error(string.Format("{0} node is empty. Ensure that {1} contains a value for {0}.", NAME_NODE, SSUP_CONFIG_FILENAME));
                                break;
                            }
                            if (node.InnerText.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
                            {
                                Logging.Error(string.Format("Value of {0} node contains invalid characters. Ensure that {0} is a valid directory name.", NAME_NODE));
                                break;
                            }
                            name = node.InnerText;
                        }
                        break;
                    case CHECK_FREQUENCY_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                Logging.Info(string.Format("{0} node is empty. Using default check frequncy.", CHECK_FREQUENCY_NODE));
                                break;
                            }
                            CheckFrequencies parseValue;
                            if (!Enum.TryParse(node.InnerText, out parseValue))
                            {
                                Logging.Warning(string.Format("{0} node is not a valid CheckFrequncy. Using default check frequncy.", CHECK_FREQUENCY_NODE));
                                break;
                            }
                            checkFrequency = parseValue;
                        }
                        break;
                    case URL_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                Logging.Error(string.Format("{0} node is empty. Ensure that {1} contains a value for {0}.", URL_NODE, SSUP_CONFIG_FILENAME));
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
                                        Logging.Error(string.Format("{0} node does not contain a valid URL. You will be unable to reach the update server.", URL_NODE));
                                        break;
                                    }
                                }
                                catch (UriFormatException)
                                {
                                    Logging.Error(string.Format("Specified update URL is either too long or improperly formatted for {0} node.", URL_NODE));
                                    break;
                                }
                                catch (Exception e)
                                {
                                    Logging.Error(string.Format("The following error occured while trying to read the value of {0}: \r\n\t{1}", URL_NODE, e.Message));
                                    break;
                                }
                            }
                            url = tempUriString;
                        }
                        break;
                }
            }
        }

        #endregion

        #region methods

        public bool IsValid()
        {
            return (!string.IsNullOrEmpty(BuildType) && !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Url));
        }

        #endregion
    }
}
