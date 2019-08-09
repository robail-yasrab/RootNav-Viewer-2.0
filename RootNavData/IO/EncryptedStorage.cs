using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.IO.IsolatedStorage;
using System.Security.Cryptography;

namespace RootNav.Data
{
    public static class EncryptedStorage
    {
        private static IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
            
        public static void SaveEncryptedString(string fileName, string data)
        {
            using (IsolatedStorageFileStream oStream = new IsolatedStorageFileStream(fileName, FileMode.Create, isoStore))
            {
                Byte[] stateBytes = Encoding.Unicode.GetBytes(data);
                Byte[] encryptedBytes = ProtectedData.Protect(stateBytes, 
                                                                null, 
                                                                DataProtectionScope.CurrentUser);
                oStream.Write(encryptedBytes, 0, encryptedBytes.Length);
            }
        }
        
        public static string ReadEncryptedString(string fileName)
        {
            String output = "";

            string[] fileNames = isoStore.GetFileNames(fileName);
            if (fileNames.Any(item => item == fileName))
            {
                //it exists
                using (IsolatedStorageFileStream iStream =
                    new IsolatedStorageFileStream(fileName, FileMode.Open, isoStore))
                {
                    MemoryStream ms = new MemoryStream();
                    iStream.CopyTo(ms);
                    var encryptedData = ms.ToArray();
                    ms.Dispose();

                    Byte[] unEncryptedBytes = ProtectedData.Unprotect(encryptedData,
                                                null,
                                                DataProtectionScope.CurrentUser);
                    output = Encoding.Unicode.GetString(unEncryptedBytes);
                }
            }
            return output;
        }
    }
}
