using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Win32
{
    public static class OpenInExplorer
    {
        public static void Open(string _path)
        {
            Debug.Assert(_path != null);

            string argument = "/select, \"" + _path + "\"";

            Process.Start("explorer.exe", argument);
        }
    }
}
