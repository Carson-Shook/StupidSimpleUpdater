using System.Xml;

namespace StupidSimpleUpdater.Utilities
{
    public class XmlUtilities
    {
        #region static methods

        // See: https://stackoverflow.com/a/16937036
        // Basically, this is the only easy way to ensure that
        // a string is a valid xml element name
        public static bool IsValidXmlNodeName(string name)
        {
            try
            {
                XmlConvert.VerifyName(name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
