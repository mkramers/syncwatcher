using System.Collections.Generic;
using CommandLine;
using CommandLine.Text;

namespace BuildTool.CommandLineDemo
{
    [Verb("getchangelist", HelpText = "Reserves a changelist in p4 and writes the changelist # to file.")]
    public class P4GetChangelistOptions
    {
        [Value(0, MetaName = "description",
            HelpText = "Changelist description.",
            Required = true)]
        public string Description { get; set; }

        [Value(1, MetaName = "outputfile",
            HelpText = "File to write changelist # to.",
            Required = true)]
        public string OutputFile { get; set; }

        [Option("quiet",
            HelpText = "Suppresses summary messages.")]
        public bool Quiet { get; set; }

        [Usage(ApplicationAlias = "P4Utility.exe")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("normal scenario", new P4GetChangelistOptions { Description = "new changelist", OutputFile = "tmp.txt"});
                yield return new Example("queit scenario", new P4GetChangelistOptions { Description = "new changelist", OutputFile = "tmp.txt", Quiet = true});
            }
        }
    }
}