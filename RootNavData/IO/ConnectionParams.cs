using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootNav.Data.IO
{
    [Serializable]
    public enum ConnectionSource
    {
        RSMLDirectory
    }

    [Serializable]
    public class ConnectionParams
    {
        public String Username { get; set; }
        public ConnectionSource Source { get; set; }

        public string Directory { get; set; }

        public string ToXML()
        {
            System.Xml.Serialization.XmlSerializer sx = new System.Xml.Serialization.XmlSerializer(this.GetType());
            StringBuilder xmlString = new StringBuilder();
            System.Xml.XmlWriter wrt = System.Xml.XmlWriter.Create(xmlString);
            sx.Serialize(wrt, this);
            return xmlString.ToString();
        }

        public static ConnectionParams FromXML(string xmlString)
        {
            try
            {
                System.IO.MemoryStream ms = new System.IO.MemoryStream(Encoding.Unicode.GetBytes(xmlString));
                System.Xml.Serialization.XmlSerializer sx = new System.Xml.Serialization.XmlSerializer(typeof(ConnectionParams));
                object param = sx.Deserialize(ms);
                return param as ConnectionParams;
            }
            catch
            {
                // If a C_DATA file exists, it's not valid XML
                return null;
            }
        }

        public static ConnectionParams FromEncryptedStorage()
        {
            // Attempt to read encrypted storage
            string xmlData = EncryptedStorage.ReadEncryptedString("C_DATA");

            if (xmlData != null && xmlData != "")
            {
                return ConnectionParams.FromXML(xmlData);
            }
            else
            {
                return null;
            }
        }
    }
}
