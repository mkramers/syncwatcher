using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using P4Commands;

namespace BuildTool
{
    internal class Program
    {
        private static void Main(string[] _args)
        {
            var p4 = new P4Executable();
            p4.Run(_args);

            //Test.Do(_args);

            //Console.ReadKey();
        }
    }
}
