using StupidSimpleUpdater.Utilities;
using System;
using System.Collections.Generic;
using System.Xml;

namespace StupidSimpleUpdater.Updater.XmlData
{
    public class SSUPUpdateManifest
    {
        #region fields/properties

        private Dictionary<string, SSUPUpdateManifestBundle> internalCollection = new Dictionary<string, SSUPUpdateManifestBundle>();
        public Dictionary<string, SSUPUpdateManifestBundle> InternalCollection { get => internalCollection; }

        #endregion

        #region indexer

        // We want to each bundle to be accessable by the name of
        // the node that it is contained in.
        public SSUPUpdateManifestBundle this[string key]
        {
            get
            {
                if (key == null)
                {
                    return null;
                }
                SSUPUpdateManifestBundle retVal;
                if (!internalCollection.TryGetValue(key, out retVal))
                {
                    return null;
                }
                return retVal;
            }
        }

        #endregion

        #region .ctor

        public SSUPUpdateManifest(string xmlText)
        {
            XmlDocument updateManifest = new XmlDocument();
            try
            {
                updateManifest.LoadXml(xmlText);
            }
            catch (XmlException)
            {
                Logging.Error(string.Format("There was a problem with the format of the Update Manifest file.\r\n\t Full manifest text: {0}", xmlText));
                return;
            }
            catch (Exception e)
            {
                Logging.Error(string.Format("The following error occured while trying to load the Update Manifest file: \r\n\t{0}", e.Message));
                return;
            }

            foreach (XmlNode node in updateManifest.DocumentElement.ChildNodes)
            {
                if (node == null)
                {
                    // We should not ever have a null value. We would have
                    // had an XmlException long before, and ChildNodes returns 
                    // an empty list if there's nothing in it anyway.
                    throw new NullReferenceException(nameof(node));
                }

                SSUPUpdateManifestBundle bundle = new SSUPUpdateManifestBundle(node);

                if (!bundle.IsValid())
                {
                    Logging.Warning(string.Format("Update bundle {0} was improperly configured. This bundle will be skipped.", node.Name));
                }
                else
                {
                    internalCollection.Add(node.Name, bundle);
                }
            }
        }

        #endregion
    }
}
