using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;
using RootNav.Data.IO;

namespace RootNav.Data
{
    /// <summary>
    /// Interface responsible for reading root data from files or databases
    /// </summary>
    public interface IRootReader
    {
        bool Initialise();
        Tuple<SceneMetadata,SceneInfo,ImageCollection> Read(String tag, bool image);
        List<String> FilterTags(String[] searchTerms, bool any, bool imageOnly, DateTime? date = null);
        List<String> ReadAllTags();
        bool Connected { get; }
    }

    /// <summary>
    /// Exception thrown when information on a tag is requested but not found
    /// </summary>
    public class TagNotFoundException : System.Exception
    {
        public TagNotFoundException(String message) 
            : base(message)
        {

        }
    }

}
