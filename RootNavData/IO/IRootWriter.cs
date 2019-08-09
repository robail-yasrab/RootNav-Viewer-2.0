using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootNav.Data
{
    /// <summary>
    /// Interface responsible for writing root data to files or databases
    /// </summary>
    public interface IRootWriter
    {
        bool Initialise();
        bool Write(SceneMetadata metadata, SceneInfo scene);
        bool Connected { get; }
    }

}
