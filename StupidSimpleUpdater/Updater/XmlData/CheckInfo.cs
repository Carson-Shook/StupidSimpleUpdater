using StupidSimpleUpdater.Utilities;
using System;
using System.IO;
using System.Xml;

namespace StupidSimpleUpdater.Updater.XmlData
{
    public class CheckInfo
    {
        #region constants

        private const string DATE_FORMAT = "yyyy-MM-dd";
        public static string ProgramDataDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\StupidSimpleUpdater\";
        public const string CHECK_INFO_FILENAME = "CheckInfo.xml";
        public const string LAST_CHECK_DATE_NODE = "LastCheckDate";
        public const string SKIP_THIS_VERSION_NODE = "SkipThisVersion";
        public const string LATEST_VERSION_NODE = "LatestVersion";

        #endregion

        #region fields/properties

        private string checkInfoPath;

        // The last date that the updater sucessfully checked for updates.
        private DateTime? lastCheckDate;
        public DateTime? LastCheckDate { get => lastCheckDate; }

        // User set value to determine whether or not the latest version
        // should be skipped.
        private bool skipThisVersion = false;
        public bool SkipThisVersion { get => skipThisVersion; set => skipThisVersion = value; }

        // The last known version. If SkipThisVersion is true, then as long
        // as the currently available version is equal to the LatestVersion,
        // then the updater will not prompt the user to update unless they
        // run the updater manually.
        private Version latestVersion = null;
        public Version LatestVersion { get => latestVersion; set => latestVersion = value; }

        #endregion

        #region .ctor

        public CheckInfo(string checkInfoPath)
        {
            this.checkInfoPath = checkInfoPath;

            XmlDocument checkInfo = new XmlDocument();
            try
            {
                checkInfo.Load(checkInfoPath);
            }
            catch (DirectoryNotFoundException)
            {
                Logging.Info(string.Format("No {0}. Will create {0} at the following location: \r\n\t{1}.", CHECK_INFO_FILENAME, checkInfoPath));
                return;
            }
            catch (FileNotFoundException)
            {
                Logging.Info(string.Format("No {0}. Will create {0} at the following location: \r\n\t{1}.", CHECK_INFO_FILENAME, checkInfoPath));
                return;
            }
            catch (XmlException)
            {
                Logging.Warning(string.Format("There was a problem with the format of your {0} file. It will be recreated.", CHECK_INFO_FILENAME));
                File.Delete(checkInfoPath);
                return;
            }
            catch (Exception e)
            {
                Logging.Error(string.Format("The following error occured while trying to load {0}: \r\n\t{1}", CHECK_INFO_FILENAME, e.Message));
                return;
            }

            // Since this is just a temp file (more or less), we don't care
            // whether or not something is wrong. We'll just skip it now
            // and overwrite it later.
            foreach (XmlNode node in checkInfo.DocumentElement.ChildNodes)
            {
                switch (node.Name)
                {
                    case LAST_CHECK_DATE_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                break;
                            }
                            DateTime parseValue;
                            if (DateTime.TryParseExact(node.InnerText, DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out parseValue))
                            {
                                lastCheckDate = parseValue;
                            }
                        }
                        break;
                    case SKIP_THIS_VERSION_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                break;
                            }
                            bool parseValue;
                            if (bool.TryParse(node.InnerText, out parseValue))
                            {
                                skipThisVersion = parseValue;
                            }
                        }
                        break;
                    case LATEST_VERSION_NODE:
                        {
                            if (string.IsNullOrEmpty(node.InnerText))
                            {
                                break;
                            }
                            Version parseValue;
                            if (Version.TryParse(node.InnerText, out parseValue))
                            {
                                latestVersion = parseValue;
                            }
                        }
                        break;
                }
            }
        }

        #endregion

        #region methods

        // I hate this method name, but I couldn't think of anything better
        // ensures that the reqested ammount of time has passed between the
        // current LastCheckDate and DateTime.Now. Always returns true if
        // LastCheckDate is null or checkFrequency is EveryStart. Returns
        // false if checkFrequency is Never.
        public bool TimeHasElapsed(CheckFrequencies checkFrequency)
        {
            if (!LastCheckDate.HasValue)
            {
                return true;
            }

            // Worth noting that update schedule is not critical enough for
            // us to bother using anything more than current local time to
            // check against.
            DateTime now = DateTime.Now.Date;
            bool retVal = false;

            Logging.Info(string.Format("Current date: {0}, Last check date: {1}", now.ToString(DATE_FORMAT), LastCheckDate.Value.ToString(DATE_FORMAT)));

            switch (checkFrequency)
            {
                case CheckFrequencies.Never:
                    retVal = false;
                    break;
                case CheckFrequencies.EveryStart:
                    retVal = true;
                    break;
                case CheckFrequencies.Daily:
                    if (LastCheckDate.Value.Date.AddDays(1).CompareTo(now) <= 0)
                    {
                        retVal = true;
                    }
                    break;
                case CheckFrequencies.Weekly:
                    if (LastCheckDate.Value.Date.AddDays(7).CompareTo(now) <= 0)
                    {
                        retVal = true;
                    }
                    break;
                case CheckFrequencies.Monthly:
                    if (LastCheckDate.Value.Date.AddMonths(1).CompareTo(now) <= 0)
                    {
                        retVal = true;
                    }
                    break;
                default:
                    // should never happen, but the universe is a strange place.
                    retVal = true;
                    break;
            }

            return retVal;
        }

        public void UpdateLastCheckDate()
        {
            lastCheckDate = DateTime.Now.Date;
        }

        public void Write()
        {
            XmlDocument checkInfo = new XmlDocument();
            checkInfo.PreserveWhitespace = false;

            XmlDeclaration xmlDeclaration = checkInfo.CreateXmlDeclaration("1.0", "utf-8", null);
            XmlElement root = checkInfo.DocumentElement;
            checkInfo.InsertBefore(xmlDeclaration, root);

            XmlElement documentElement = checkInfo.CreateElement("CheckInfo");
            checkInfo.AppendChild(documentElement);

            if (LastCheckDate.HasValue)
            {
                XmlElement lastCheckDateNode = checkInfo.CreateElement(LAST_CHECK_DATE_NODE);
                lastCheckDateNode.AppendChild(checkInfo.CreateTextNode(LastCheckDate.Value.ToString(DATE_FORMAT)));
                documentElement.AppendChild(lastCheckDateNode);
            }

            XmlElement skipThisVersionNode = checkInfo.CreateElement(SKIP_THIS_VERSION_NODE);
            skipThisVersionNode.AppendChild(checkInfo.CreateTextNode(SkipThisVersion.ToString()));
            documentElement.AppendChild(skipThisVersionNode);

            if (LatestVersion != null)
            {
                XmlElement latestVersionNode = checkInfo.CreateElement(LATEST_VERSION_NODE);
                latestVersionNode.AppendChild(checkInfo.CreateTextNode(LatestVersion.ToString()));
                documentElement.AppendChild(latestVersionNode);
            }

            FileInfo fileInfo = new FileInfo(checkInfoPath);

            if (!Directory.Exists(fileInfo.Directory.ToString()))
            {
                fileInfo.Directory.Create();
            }
            checkInfo.Save(checkInfoPath);
        }

        #endregion
    }
}
