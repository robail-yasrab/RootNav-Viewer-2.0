using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;



namespace RootNav.Data
{
    public class SplineSerializer
    {
        public static byte[] ObjectToBinary(object o)
        {
            MemoryStream oStream = new MemoryStream();
            BinaryFormatter oBF = new BinaryFormatter();
            oBF.Serialize(oStream, o);
            byte[] arrBuffer = new byte[oStream.Length];
            oStream.Seek(0, SeekOrigin.Begin);
            oStream.Read(arrBuffer, 0, arrBuffer.Length);
            oStream.Close();
            return arrBuffer;
        }

        public static object BinaryToObject(byte[] binary)
        {
            if (binary != null)
            {
                IFormatter formatter = new BinaryFormatter();
                (formatter as BinaryFormatter).AssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
                formatter.Binder = new BackwardsCompatibleDeserializationBinder();

                return formatter.Deserialize(new MemoryStream(binary));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Necessary to handle the change in location of the Spline, SampledSpline and SplinePositionReference classes
        /// </summary>
        private sealed class BackwardsCompatibleDeserializationBinder : System.Runtime.Serialization.SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;

                // Handle older refinements saved under the original namespace
                if (assemblyName.Contains("RootNav,"))
                {
                    assemblyName = assemblyName.Replace("RootNav", "RootNav.Data");
                    typeName = typeName.Replace("Rootnav.Core.Measurement.", "RootNav.Data.");
                }
                // If required, insert future namespace alterations here
                else if (assemblyName.Contains("RootNavMeasurement,"))
                {
                    assemblyName = assemblyName.Replace("RootNavMeasurement", "RootNav.Data");
                    typeName = typeName.Replace("RootNavMeasurement.", "RootNav.Data.");
                }
                else if (assemblyName.Contains("RootNav.Measurement,"))
                {
                    assemblyName = assemblyName.Replace("RootNav.Measurement", "RootNav.Data");
                    typeName = typeName.Replace("RootNav.Measurement.", "RootNav.Data.");
                }

                // Get the type using the typeName and assemblyName
                typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                    typeName, assemblyName));

                return typeToDeserialize;
            }
        }

    }
}
