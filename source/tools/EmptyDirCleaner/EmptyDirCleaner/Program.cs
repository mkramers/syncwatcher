using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmptyDirCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            ProcessDirectory(@"D:\workp4\Projects");
        }

        private static void ProcessDirectory(string _startLocation)
        {
            foreach (var directory in Directory.GetDirectories(_startLocation))
            {
                ProcessDirectory(directory);

                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Console.WriteLine($"Deleting: {directory}");
                    Directory.Delete(directory, false);
                }
            }
        }
    }
}
