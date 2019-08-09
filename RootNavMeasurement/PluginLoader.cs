using System;
using System.Collections.Generic;
using System.Windows;
using System.Reflection;
using System.IO;

namespace RootNav.Measurement
{
    public class PluginLoader
    {
        /// <summary>
        /// Returns a list of compatible plugins contained in any .dll files in a given directory
        /// </summary>
        /// <typeparam name="T">The abstract class required</typeparam>
        /// <param name="folder">The path in which any compatible dlls will be found</param>
        /// <returns>A list of classes that implement the interface T</returns>
        public static List<T> GetPlugins<T>(string folder)
        {

            string[] files = Directory.GetFiles(folder, "*.dll");

            List<T> tList = new List<T>();

            if (!typeof(T).IsAbstract)
                throw new Exception("Class T is not an abstract class");

            foreach (string file in files)
            {
                // Specific rules to save processing time
                if (System.IO.Path.GetFileName(file) == "mysql.data.dll" ||
                    System.IO.Path.GetFileName(file) == "System.Windows.Controls.DataVisualization.Toolkit.dll" ||
                    System.IO.Path.GetFileName(file) == "WPFToolkit.dll")
                    continue;

                try
                {
                    Assembly assembly = Assembly.LoadFile(file);
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (!type.IsClass || type.IsNotPublic) continue;

                        if (type.BaseType == typeof(T))
                        {
                            object obj = Activator.CreateInstance(type);

                            T t = (T)obj;

                            tList.Add(t);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return tList;
        }

    }
}
