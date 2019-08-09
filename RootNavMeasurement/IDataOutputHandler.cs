using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootNav.Measurement
{
    public interface IDataOutputHandler
    {
        void Clear();

        void Add(Dictionary<string, string> measurement);
    }
}